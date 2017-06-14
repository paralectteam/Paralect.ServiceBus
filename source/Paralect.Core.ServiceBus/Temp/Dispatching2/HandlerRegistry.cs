using System;
using System.Collections.Generic;
using System.Linq;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class HandlerRegistry : IHandlerRegistry
    {
        /// <summary>
        /// Handlers lookup dictionary. Handlers already in the order.
        /// </summary>
        private readonly List<IHandler> _handlers;

        /// <summary>
        /// Creates HandlerRegistry with specified lookup dictionary
        /// </summary>
        public HandlerRegistry(List<IHandler> handlers)
        {
            _handlers = handlers;
        }

        /// <summary>
        /// Returns handlers in a correct order. 
        /// Returns empty array if no handlers found for such message.
        /// This method doesn't use reflection.
        /// </summary>
        public IHandler[] GetHandlers(Type messageType)
        {
            return _handlers.Where(handler => IsMatch(messageType, handler.Subscriptions)).ToArray();
        }

        public bool IsMatch(Type messageType, IEnumerable<Type> handlerSubscriptions)
        {
            foreach (var handlerSubscription in handlerSubscriptions)
            {
                if (messageType == handlerSubscription)
                    return true;

                if (handlerSubscription.IsAssignableFrom(messageType))
                    return true;
            }

            return false;
        }
    }
}