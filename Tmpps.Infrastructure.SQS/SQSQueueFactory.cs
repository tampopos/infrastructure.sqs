using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Tmpps.Infrastructure.Common.Foundation.Exceptions;
using Tmpps.Infrastructure.SQS.Interfaces;
using Tmpps.Infrastructure.SQS.Models;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSQueueFactory : ISQSQueueFactory
    {
        private ISQSClientProvider sqsClientProvider;
        private CancellationTokenSource tokenSource;

        public SQSQueueFactory(ISQSClientProvider sqsClientProvider)
        {
            this.sqsClientProvider = sqsClientProvider;
        }

        public SQSQueueFactory(
            ISQSClientProvider sqsClientProvider,
            CancellationTokenSource tokenSource
        ) : this(sqsClientProvider)
        {
            this.tokenSource = tokenSource;
        }

        private IAmazonSQS SQSClient => this.sqsClientProvider.SQSClient;
        private CancellationToken Token => this.tokenSource?.Token ?? CancellationToken.None;

        public async Task<string> GetQueueUrlAsync(string queueName)
        {
            try
            {
                var response = await this.SQSClient.GetQueueUrlAsync(queueName, this.Token);
                return response.QueueUrl;
            }
            catch (QueueDoesNotExistException)
            {
                return null;
            }
        }
        public async Task<string> CreateQueueAsync(SQSQueue queue)
        {
            var request = new CreateQueueRequest();
            request.QueueName = queue.Name;
            if (queue.IsFifo)
            {
                request.Attributes.Add("FifoQueue", true.ToString());
                request.Attributes.Add("ContentBasedDeduplication", true.ToString());
                if (!request.QueueName.EndsWith(".fifo"))
                {
                    request.QueueName += ".fifo";
                }
            }
            request.Attributes.Add(nameof(queue.DelaySeconds), queue.DelaySeconds.ToString());
            request.Attributes.Add(nameof(queue.MaximumMessageSize), queue.MaximumMessageSize.ToString());
            request.Attributes.Add(nameof(queue.MessageRetentionPeriod), queue.MessageRetentionPeriod.ToString());
            request.Attributes.Add(nameof(queue.ReceiveMessageWaitTimeSeconds), queue.ReceiveMessageWaitTimeSeconds.ToString());
            request.Attributes.Add(nameof(queue.VisibilityTimeout), queue.VisibilityTimeout.ToString());
            var response = await this.SQSClient.CreateQueueAsync(request, this.Token);
            return response?.QueueUrl;
        }
        public async Task SetQueueAttributesAsync(string queueUrl, SQSQueue queue)
        {
            var request = new SetQueueAttributesRequest();
            request.QueueUrl = queueUrl;
            request.Attributes.Add(nameof(queue.DelaySeconds), queue.DelaySeconds.ToString());
            request.Attributes.Add(nameof(queue.MaximumMessageSize), queue.MaximumMessageSize.ToString());
            request.Attributes.Add(nameof(queue.MessageRetentionPeriod), queue.MessageRetentionPeriod.ToString());
            request.Attributes.Add(nameof(queue.ReceiveMessageWaitTimeSeconds), queue.ReceiveMessageWaitTimeSeconds.ToString());
            request.Attributes.Add(nameof(queue.VisibilityTimeout), queue.VisibilityTimeout.ToString());
            await this.SQSClient.SetQueueAttributesAsync(request, this.Token);
        }
        public async Task<string> GetQueueArnAsync(string queueName)
        {
            var url = await this.GetQueueUrlAsync(queueName);
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            var request = new GetQueueAttributesRequest();
            request.QueueUrl = url;
            request.AttributeNames.Add("QueueArn");
            var response = await this.SQSClient.GetQueueAttributesAsync(request, this.Token);
            return response?.QueueARN;
        }
        public async Task SetDeadLetterSettingsAsync(string queueUrl, SQSQueue queue)
        {
            if (string.IsNullOrEmpty(queue.DeadLetterQueueName))
            {
                var request = new SetQueueAttributesRequest();
                request.QueueUrl = queueUrl;
                request.Attributes.Add("RedrivePolicy", string.Empty);
                await this.SQSClient.SetQueueAttributesAsync(request, this.Token);
            }
            else
            {
                var arn = await this.GetQueueArnAsync(queue.DeadLetterQueueName);
                if (string.IsNullOrEmpty(arn))
                {
                    throw new BizLogicException($"DeadLetterQueueの取得に失敗しました。({queue.DeadLetterQueueName})");
                }
                var request = new SetQueueAttributesRequest();
                request.QueueUrl = queueUrl;
                var redrivePolicy = new
                {
                    deadLetterTargetArn = arn,
                    maxReceiveCount = queue.MaxReceiveCount
                };
                request.Attributes.Add("RedrivePolicy", JsonConvert.SerializeObject(redrivePolicy));
                await this.SQSClient.SetQueueAttributesAsync(request, this.Token);
            }
        }

        public async Task DeleteQueueAsync(string queueName)
        {
            var url = await this.GetQueueUrlAsync(queueName);
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            await this.SQSClient.DeleteQueueAsync(url, this.Token);
        }
    }
}