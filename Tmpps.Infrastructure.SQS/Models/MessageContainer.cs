using System;
using Newtonsoft.Json;
using Tmpps.Infrastructure.SQS.Common;

namespace Tmpps.Infrastructure.SQS.Models
{
    public class MessageContainer
    {
        public MessageContainer()
        {

        }
        public MessageContainer(Guid messageId, SQSMessageSendSetting sendSetting, string body) : this()
        {
            this.MessageId = messageId;
            this.Name = sendSetting.Name;
            this.Duration = sendSetting.Duration;
            this.DelayType = sendSetting.DelayType;
            this.Body = body;
        }
        public Guid MessageId { get; set; }
        public string Name { get; set; }
        public int Duration { get; set; }
        public SQSDelayType DelayType { get; set; }
        public string Body { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}