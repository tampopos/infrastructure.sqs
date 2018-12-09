using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tmpps.Infrastructure.Common.DependencyInjection.Interfaces;
using Tmpps.Infrastructure.Common.Foundation.Exceptions;
using Tmpps.Infrastructure.Common.Foundation.Interfaces;
using Tmpps.Infrastructure.Common.ValueObjects;
using Tmpps.Infrastructure.SQS;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSSubscriber : IMessageSubscriber, IDisposable
    {
        private ISQSConfig config;
        private ISystemClock clock;
        private ILogger logger;
        private ISQSClientProvider sqsClientProvider;
        private ISendMessageRequestFactory sendMessageRequestFactory;
        private CancellationTokenSource tokenSource;
        private ITypeHelper typeHelper;
        private IScopeProvider scopeProvider;
        private SemaphoreSlim semaphoreSlim;

        public SQSSubscriber(
            ISQSConfig config,
            ILogger logger,
            ISystemClock clock,
            ISQSClientProvider sqsClientProvider,
            ISendMessageRequestFactory sendMessageRequestFactory,
            CancellationTokenSource tokenSource,
            ITypeHelper typeHelper,
            IScopeProvider scopeProvider)
        {
            this.config = config;
            this.clock = clock;
            this.logger = logger;
            this.sqsClientProvider = sqsClientProvider;
            this.sendMessageRequestFactory = sendMessageRequestFactory;
            this.tokenSource = tokenSource;
            this.typeHelper = typeHelper;
            this.scopeProvider = scopeProvider;
            this.semaphoreSlim = config.MaxConcurrencyReceive > 0 ? new SemaphoreSlim(config.MaxConcurrencyReceive) : null;
        }

        private IAmazonSQS SQSClient => this.sqsClientProvider.SQSClient;

        public async Task SubscribeAsync()
        {
            var targets = this.config.SQSMessageReceiveSettings
                .Select(x => x.Value);
            var tasks = targets.SelectMany(setting =>
            {
                return Enumerable.Range(0, setting.InstanceCount).Select(i =>
                {
                    return this.SubscribeAsync(setting);
                });
            });

            await Task.WhenAll(tasks);
        }

        private async Task SubscribeAsync(SQSMessageReceiveSetting setting)
        {
            var queueInfo = await this.GetQueueInfoAsync(setting);
            while (!this.tokenSource.Token.IsCancellationRequested)
            {
                await (this.semaphoreSlim?.WaitAsync() ?? Task.Delay(0));
                await this.ProcessMessageAsync(setting, queueInfo);
                this.semaphoreSlim?.Release();
            }
        }

        private async Task ProcessMessageAsync(SQSMessageReceiveSetting setting, GetQueueAttributesResponse queueInfo)
        {
            var receiveMessageResponse = await this.ReceiveMessageAsync(setting);
            if ((receiveMessageResponse?.Messages?.Count ?? 0) == 0)
            {
                return;
            }
            receiveMessageResponse.Messages.AsParallel().ForAll(async message =>
            {
                var container = JsonConvert.DeserializeObject(message.Body, typeof(MessageContainer)) as MessageContainer;
                try
                {
                    if (container == null)
                    {
                        throw new BizLogicException($"message.Bodyのデコードに失敗しました。{message.MessageId} at {DateTime.Now}");
                    }
                    using(var source = new CancellationTokenSource(queueInfo.VisibilityTimeout * 900))
                    {
                        this.logger.LogInformation($"Receive {message.MessageId} at {DateTime.Now}");
                        var result = await this.ExecuteAsync(container, setting, source);
                        if (result != 0)
                        {
                            throw new BizLogicException($"Handling Error {message.MessageId} at {DateTime.Now}(code:{result})");
                        }
                        await this.DeleteMessageAsync(setting, message);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        this.logger.LogError($"Time out {message.MessageId} at {DateTime.Now}");
                    }
                    else
                    {
                        this.logger.LogError(ex, $"Error {message.MessageId} at {DateTime.Now}");
                    }
                    await this.NoticeFailureAsync(setting, message, container);
                }
            });
        }

        private async Task DeleteMessageAsync(SQSMessageReceiveSetting setting, Message message, int count = 0)
        {
            try
            {
                var deleteMessageRequest = new DeleteMessageRequest(setting.QueueUrl, message.ReceiptHandle);
                await this.SQSClient.DeleteMessageAsync(deleteMessageRequest);
                this.logger.LogInformation($"Delete message (id:{message.MessageId})");
            }
            catch (Exception ex)
            {
                await this.DeleteMessageAsync(setting, message, count++);
                if (count >= 10)
                {
                    this.logger.LogCritical(ex, $"Critical Error Can't delete message (id:{message.MessageId})");
                    throw;
                }
            }
        }

        private async Task NoticeFailureAsync(SQSMessageReceiveSetting setting, Message message, MessageContainer container, int count = 0)
        {
            try
            {
                var delay = this.ComputeDelaySeconds(message, container);
                var changeRequest = new ChangeMessageVisibilityRequest(setting.QueueUrl, message.ReceiptHandle, delay);
                await this.SQSClient.ChangeMessageVisibilityAsync(changeRequest);
                this.logger.LogInformation($"Change visibility message (id:{message.MessageId})");
            }
            catch (Exception ex)
            {
                await this.NoticeFailureAsync(setting, message, container, count++);
                if (count >= 10)
                {
                    this.logger.LogCritical(ex, $"Critical Error Can't change visibility message (id:{message.MessageId})");
                    throw;
                }
            }
        }

        private int ComputeDelaySeconds(Message message, MessageContainer container)
        {
            if (container == null || container.Duration == 0 || !int.TryParse(message.Attributes[SQSConstans.ApproximateReceiveCountKey], out var count) || count == 0)
            {
                return 0;
            }
            switch (container.DelayType)
            {
                case SQSDelayType.Constant:
                    return container.Duration;
                case SQSDelayType.LinerIncrease:
                    {
                        var tmp = container.Duration * (count - 1);
                        return tmp > SQSConstans.MaxDelay ? SQSConstans.MaxDelay : tmp;
                    }
                case SQSDelayType.ExponentialIncrease:
                    {
                        var tmp = container.Duration * Math.Exp(count - 1);
                        return tmp > SQSConstans.MaxDelay ? SQSConstans.MaxDelay : (int) Math.Truncate(tmp);
                    }
            }
            return 0;
        }

        private async Task<int> ExecuteAsync(MessageContainer messageContainer, SQSMessageReceiveSetting setting, CancellationTokenSource source)
        {
            if (!setting.MappingTypes.TryGetValue(messageContainer.Name, out var typeName))
            {
                throw new SQSReceiveSettingNotFoundException(messageContainer.Name);
            }
            var type = this.typeHelper.GetType(x => x.FullName == typeName);
            var obj = JsonConvert.DeserializeObject(messageContainer.Body, type);
            var executerType = typeof(IMessageReceiver<>).MakeGenericType(type);
            var inheritTokenSource = new TypeValuePair(source);
            using(var scope = this.scopeProvider.BeginLifetimeScope(inheritTokenSource))
            {
                var pair = new TypeValuePair(obj, type);
                var executer = scope.Resolve(executerType, pair) as IMessageReceiver;
                return await executer.ExecuteAsync();
            }
        }

        private async Task<GetQueueAttributesResponse> GetQueueInfoAsync(SQSMessageReceiveSetting setting)
        {
            return await this.SQSClient.GetQueueAttributesAsync(setting.QueueUrl, new List<string> { SQSConstans.VisibilityTimeoutKey }, this.tokenSource.Token);
        }

        private async Task<ReceiveMessageResponse> ReceiveMessageAsync(SQSMessageReceiveSetting setting)
        {
            var receiveMessageRequest = new ReceiveMessageRequest(setting.QueueUrl)
            {
                AttributeNames = { SQSConstans.ApproximateReceiveCountKey },
            };

            return await this.SQSClient.ReceiveMessageAsync(receiveMessageRequest, this.tokenSource.Token);
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.semaphoreSlim?.Dispose();
                }
                this.disposedValue = true;
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
        }
        #endregion
    }
}