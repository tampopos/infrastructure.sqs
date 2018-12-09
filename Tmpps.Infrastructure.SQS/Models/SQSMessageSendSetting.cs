using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Tmpps.Infrastructure.SQS.Common;

namespace Tmpps.Infrastructure.SQS.Models
{
    public class SQSMessageSendSetting
    {
        public SQSMessageSendSetting(IConfiguration configuration)
        {
            this.Name = configuration.GetValue<string>(nameof(this.Name));
            this.QueueUrl = configuration.GetValue<string>(nameof(this.QueueUrl));
            this.Duration = configuration.GetValue(nameof(this.Duration), 0);
            this.DelayType = configuration.GetValue(nameof(this.DelayType), SQSDelayType.Constant);
            this.MessageContentType = configuration.GetValue<string>(nameof(this.MessageContentType));
        }
        public string Name { get; }
        public string MessageContentType { get; }
        public string QueueUrl { get; }
        public int Duration { get; }
        public SQSDelayType DelayType { get; }
    }
}