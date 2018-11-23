using System;
using System.Collections.Generic;
using Amazon.SQS.Model;
using Tmpps.Infrastructure.SQS;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface ISQSHelper
    {
        Dictionary<string, MessageAttributeValue> CreateMessageAttributes(SQSMessageSendSetting sendSetting);
    }
}