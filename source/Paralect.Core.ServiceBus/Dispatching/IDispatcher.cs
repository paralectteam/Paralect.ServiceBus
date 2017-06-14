using System;

namespace Paralect.Core.ServiceBus.Dispatching
{
    public interface IDispatcher
    {
        void Dispatch(object message);
    }
}