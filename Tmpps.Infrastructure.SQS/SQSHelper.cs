using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS.Model;
using Tmpps.Infrastructure.SQS;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSHelper : ISQSHelper
    {
        public Dictionary<string, MessageAttributeValue> CreateMessageAttributes(SQSMessageSendSetting sendSetting)
        {
            return Create().ToDictionary(x => x.key, x => x.value);
            IEnumerable < (string key, MessageAttributeValue value) > Create()
            {
                yield return (SQSConstans.NameKey, CreateMessageAttributeValue(sendSetting.Name));
                yield return (SQSConstans.DurationKey, CreateMessageAttributeValue(sendSetting.Duration.ToString()));
                yield return (SQSConstans.DelayTypeKey, CreateMessageAttributeValue(sendSetting.DelayType.ToString()));
            }
            MessageAttributeValue CreateMessageAttributeValue(string v)
            {
                return new MessageAttributeValue()
                {
                    DataType = nameof(String),
                        StringValue = v,
                };
            }
        }
    }
}