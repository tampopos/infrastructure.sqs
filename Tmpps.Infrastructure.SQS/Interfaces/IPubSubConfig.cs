namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface IPubSubConfig
    {
        string ProjectId { get; }
        string TopicId { get; }
        string SubscriptionId { get; }
    }
}