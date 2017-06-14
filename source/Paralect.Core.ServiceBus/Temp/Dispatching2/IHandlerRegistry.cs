using System;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public interface IHandlerRegistry
    {
        /// <summary>
        /// Returns descriptors in a correct order.
        /// </summary>
        IHandler[] GetHandlers(Type messageType);
    }
}