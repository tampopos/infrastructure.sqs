using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tmpps.Infrastructure.Common.Foundation.Exceptions;
using Tmpps.Infrastructure.Common.Foundation.Interfaces;
using Tmpps.Infrastructure.SQS;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSMessageSender : IMessageSender
    {
        private ISQSConfig config;
        private ISystemClock clock;
        private ILogger logger;
        private ISQSClientProvider sqsClientProvider;
        private ISendMessageRequestFactory sendMessageRequestFactory;

        public SQSMessageSender(
            ISQSConfig config,
            ILogger logger,
            ISystemClock clock,
            ISQSClientProvider sqsClientProvider,
            ISendMessageRequestFactory sendMessageRequestFactory)
        {
            this.config = config;
            this.clock = clock;
            this.logger = logger;
            this.sqsClientProvider = sqsClientProvider;
            this.sendMessageRequestFactory = sendMessageRequestFactory;
        }

        private IAmazonSQS SQSClient => this.sqsClientProvider.SQSClient;

        public async Task<string> SendAsync<T>(T message) where T : class
        {
            var sendMessageRequest = this.sendMessageRequestFactory.CreateSendMessage(message);
            var sendMessageResponse = await this.SQSClient.SendMessageAsync(sendMessageRequest);
            this.logger.LogInformation($"Send {sendMessageResponse.MessageId} at {DateTime.Now}:{typeof(T).FullName}");
            return sendMessageResponse.MessageId;
        }
    }
}