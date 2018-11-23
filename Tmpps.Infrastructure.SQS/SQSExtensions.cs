using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Tmpps.Infrastructure.SQS
{
    public static class SQSExtensions
    {
        public static IDictionary<string, SQSMessageSendSetting> GetSQSMessageSendSettings(this IConfigurationRoot configurationRoot, string sectionName)
        {
            return configurationRoot
                .GetSection(sectionName)
                .GetChildren()
                .Select(x => new SQSMessageSendSetting(x))
                .ToDictionary(x => x.MessageContentType);
        }
        public static IDictionary<string, SQSMessageReceiveSetting> GetSQSMessageReceiveSettings(this IConfigurationRoot configurationRoot, string sectionName)
        {
            return configurationRoot
                .GetSection(sectionName)
                .GetChildren()
                .Select(x => new SQSMessageReceiveSetting(x))
                .ToDictionary(x => x.QueueUrl);
        }
    }
}