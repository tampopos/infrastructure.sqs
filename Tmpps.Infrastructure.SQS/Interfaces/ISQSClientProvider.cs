using Amazon.SQS;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface ISQSClientProvider
    {
        IAmazonSQS SQSClient { get; }
    }
}