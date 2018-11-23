namespace Tmpps.Infrastructure.SQS
{
    public static class SQSConstans
    {
        public const int MaxDelay = 900;
        public const string NameKey = "sqs-name";
        public const string DurationKey = "sqs-duration";
        public const string DelayTypeKey = "sqs-delay-type";
        public const string ApproximateReceiveCountKey = "ApproximateReceiveCount";
        public const string VisibilityTimeoutKey = "VisibilityTimeout";
    }
}