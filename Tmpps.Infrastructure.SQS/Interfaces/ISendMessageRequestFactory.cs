using System;
using Amazon.SQS.Model;

namespace Tmpps.Infrastructure.SQS.Interfaces
{
    public interface ISendMessageRequestFactory
    {
        SendMessageRequest CreateSendMessage(object message, Type type, int receiveCount = 0);
        SendMessageRequest CreateSendMessage(string message, Type type, int receiveCount = 0);
        SendMessageRequest CreateSendMessage<T>(T message, int receiveCount = 0) where T : class;
    }
}