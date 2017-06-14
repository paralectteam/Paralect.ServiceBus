using Paralect.Core.ServiceBus.Dispatching;

namespace Paralect.Core.ServiceBus
{
    public interface IMessageHandlerInterceptor
    {
        void Intercept(DispatcherInvocationContext context);
    }
}
