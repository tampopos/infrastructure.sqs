using Tmpps.Infrastructure.Common.DependencyInjection.Builder.Interfaces;
using Tmpps.Infrastructure.SQS.Interfaces;

namespace Tmpps.Infrastructure.SQS
{
    public class SQSDIModule : IDIModule
    {
        public void DefineModule(IDIBuilder builder)
        {
            builder.RegisterType<SendMessageRequestFactory>(x => x.As<ISendMessageRequestFactory>().SingleInstance());
            builder.RegisterType<SQSClientProvider>(x => x.As<ISQSClientProvider>().SingleInstance());
            builder.RegisterType<SQSMessageSender>(x => x.As<IMessageSender>().SingleInstance());
            builder.RegisterType<SQSSubscriber>(x => x.As<IMessageSubscriber>().SingleInstance());
        }
    }
}