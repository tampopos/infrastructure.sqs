using System.Threading.Tasks;
using Tmpps.Infrastructure.SQS.Models;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface ISQSQueueFactory
    {
        Task<string> GetQueueUrlAsync(string queueName);
        Task<string> CreateQueueAsync(SQSQueue queue);
        Task SetQueueAttributesAsync(string queueUrl, SQSQueue queue);
        Task<string> GetQueueArnAsync(string queueName);
        Task SetDeadLetterSettingsAsync(string queueUrl, SQSQueue queue);
        Task DeleteQueueAsync(string queueName);
    }
}