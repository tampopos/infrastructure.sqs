namespace Tmpps.Infrastructure.SQS.Common
{
    public enum SQSDelayType
    {
        Constant,
        LinerIncrease,
        ExponentialIncrease,
    }
}