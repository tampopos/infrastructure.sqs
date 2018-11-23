using System.Threading.Tasks;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface IMessageSubscriber
    {
        Task SubscribeAsync();
    }
}