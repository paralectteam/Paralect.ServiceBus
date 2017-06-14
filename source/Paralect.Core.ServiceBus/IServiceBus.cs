using System;

namespace Paralect.Core.ServiceBus
{
    public interface IServiceBus : IDisposable
    {
        void Send(params object[] messages);
        void SendLocal(params object[] messages);
        void Publish(object message);

        Exception GetLastException();
        void Run();
        void Wait();
    }
}