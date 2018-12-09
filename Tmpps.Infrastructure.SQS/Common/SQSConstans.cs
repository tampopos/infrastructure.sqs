namespace Tmpps.Infrastructure.SQS.Common
{
    public static class SQSConstans
    {
        public const int MaxDelay = 900;
        public const string ApproximateReceiveCountKey = "ApproximateReceiveCount";
        public const string VisibilityTimeoutKey = "VisibilityTimeout";
    }
}