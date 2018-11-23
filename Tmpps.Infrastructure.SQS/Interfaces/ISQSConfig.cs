using System.Collections.Generic;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface ISQSConfig
    {
        string AwsAccessKeyId { get; }
        string AwsSecretAccessKey { get; }
        string ServiceURL { get; }
        IDictionary<string, SQSMessageSendSetting> SQSMessageSendSettings { get; }
        IDictionary<string, SQSMessageReceiveSetting> SQSMessageReceiveSettings { get; }

        int MaxConcurrencyReceive { get; }
    }
}