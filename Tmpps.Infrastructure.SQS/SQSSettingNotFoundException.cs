using Tmpps.Infrastructure.Common.Foundation.Exceptions.Bases;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSSendSettingNotFoundException : TmppsException
    {
        public SQSSendSettingNotFoundException(string key) : base($"SQSSendSettingが存在しません。(key:{key})") { }
    }
    public class SQSReceiveSettingNotFoundException : TmppsException
    {
        public SQSReceiveSettingNotFoundException(string key) : base($"SQSReceiveSettingが存在しません。(key:{key})") { }
    }
}