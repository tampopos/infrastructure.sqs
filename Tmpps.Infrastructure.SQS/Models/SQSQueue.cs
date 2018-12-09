namespace Tmpps.Infrastructure.SQS.Models
{
    public class SQSQueue
    {
        public string Name { get; set; }
        public int DelaySeconds { get; set; } = 0;
        public int MaximumMessageSize { get; set; } = 262144;
        public int MessageRetentionPeriod { get; set; } = 345600;
        public int ReceiveMessageWaitTimeSeconds { get; set; } = 0;
        public int VisibilityTimeout { get; set; } = 600;
        public string DeadLetterQueueName { get; set; }
        public int MaxReceiveCount { get; set; } = 10;
        public bool IsFifo { get; set; }
    }
}