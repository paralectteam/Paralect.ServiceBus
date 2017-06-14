using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class HandlerRegistryBuilder : IHandlerRegistryBuilder
    {
        /// <summary>
        /// Registered handlers
        /// </summary>
        private readonly OrderedDictionary/* <IHandler, null> */ _handlers = new OrderedDictionary(100);

        /// <summary>
        /// Register handler
        /// </summary>
        public void Register(IHandler handler)
        {
            // Check that handler wasn't registered before
            if (_handlers.Contains(handler))
                throw new Exception($"Handler {handler.Name} already registered.");

            _handlers.Add(handler, null);
        }

        /// <summary>
        /// Unregister handler
        /// </summary>
        public void Unregister(IHandler handler)
        {
            if (!_handlers.Contains(handler))
                throw new Exception(
                    $"Cannot unregister not registered handler. Handler {handler.Name} wasn't registered.");

            _handlers.Remove(handler);
        }

        /// <summary>
        /// Build handler registry
        /// </summary>
        public IHandlerRegistry BuildHandlerRegistry()
        {
            var handlers = new List<IHandler>(_handlers.Count);
            handlers.AddRange(from DictionaryEntry entry in _handlers select (IHandler) entry.Key);

            return new HandlerRegistry(handlers);
        }
    }
}