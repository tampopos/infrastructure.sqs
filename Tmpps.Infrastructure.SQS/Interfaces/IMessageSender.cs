using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface IMessageSender
    {
        Task<string> SendAsync<T>(T message) where T : class;
    }
}