using System;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Tmpps.Infrastructure.Common.Foundation.Interfaces;
using Tmpps.Infrastructure.SQS;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SendMessageRequestFactory : ISendMessageRequestFactory
    {
        private ISQSConfig config;
        private IGuidFactory guidFactory;

        public SendMessageRequestFactory(ISQSConfig config, IGuidFactory guidFactory)
        {
            this.config = config;
            this.guidFactory = guidFactory;
        }
        public SendMessageRequest CreateSendMessage(object message, Type type, int receiveCount = 0)
        {
            var json = JsonConvert.SerializeObject(message);
            return this.CreateSendMessage(json, type, receiveCount);
        }
        public SendMessageRequest CreateSendMessage(string body, Type type, int receiveCount = 0)
        {
            if (!config.SQSMessageSendSettings.TryGetValue(type.FullName, out var setting))
            {
                throw new SQSSendSettingNotFoundException(type.FullName, body);
            }
            var message = new MessageContainer(this.guidFactory.CreateNew(), setting, body);
            var sendMessageRequest = new SendMessageRequest(setting.QueueUrl, message.ToJson());
            return sendMessageRequest;
        }

        public SendMessageRequest CreateSendMessage<T>(T message, int receiveCount = 0) where T : class
        {
            return this.CreateSendMessage(message, typeof(T), receiveCount);
        }
    }
}