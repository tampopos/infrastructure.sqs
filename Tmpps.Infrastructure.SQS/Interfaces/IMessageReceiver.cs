using System.Threading.Tasks;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface IMessageReceiver
    {
        Task<int> ExecuteAsync();
    }
    public interface IMessageReceiver<T> : IMessageReceiver where T : class { }
}