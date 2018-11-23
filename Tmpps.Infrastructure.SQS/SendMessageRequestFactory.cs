using System;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Tmpps.Infrastructure.SQS;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SendMessageRequestFactory : ISendMessageRequestFactory
    {
        private ISQSConfig config;
        private ISQSHelper sqsHelper;

        public SendMessageRequestFactory(ISQSConfig config, ISQSHelper sqsHelper)
        {
            this.config = config;
            this.sqsHelper = sqsHelper;
        }
        public SendMessageRequest CreateSendMessage(object message, Type type, int receiveCount = 0)
        {
            var json = JsonConvert.SerializeObject(message);
            return this.CreateSendMessage(json, type, receiveCount);
        }
        public SendMessageRequest CreateSendMessage(string message, Type type, int receiveCount = 0)
        {
            if (!config.SQSMessageSendSettings.TryGetValue(type.FullName, out var setting))
            {
                throw new SQSSendSettingNotFoundException(type.FullName);
            }
            var sendMessageRequest = new SendMessageRequest(setting.QueueUrl, message)
            {
                MessageAttributes = this.sqsHelper.CreateMessageAttributes(setting),
            };
            return sendMessageRequest;
        }

        public SendMessageRequest CreateSendMessage<T>(T message, int receiveCount = 0) where T : class
        {
            return this.CreateSendMessage(message, typeof(T), receiveCount);
        }
    }
}