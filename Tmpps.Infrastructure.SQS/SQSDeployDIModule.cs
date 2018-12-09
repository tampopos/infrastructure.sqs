using Tmpps.Infrastructure.Common.DependencyInjection.Builder.Interfaces;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSDeployDIModule : IDIModule
    {
        public void DefineModule(IDIBuilder builder)
        {
            builder.RegisterType<SQSQueueFactory>(x => x.As<ISQSQueueFactory>().SingleInstance());
        }
    }
}