using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class DelegateHandler : IHandler
    {
        private readonly Action<object> _shortAction;
        private readonly Action<object, IServiceLocator> _fullAction;

        private readonly List<Type> _messageTypes;
        private readonly object _key;
        private readonly DispatchMode _dispatchMode;

        public DelegateHandler(Action<object> shortAction, object key, DispatchMode mode, IReadOnlyCollection<Type> messageTypes)
        {
            if (messageTypes.Count == 0)
                throw new Exception("Empty list of subscribed messages for delegate handler. Should be at least one message.");

            _shortAction = shortAction ?? throw new ArgumentNullException(nameof(shortAction));
            _messageTypes = new List<Type>(messageTypes);
            _dispatchMode = mode;

            // If key wasn't specified, use delegate instance as key.
            _key = key ?? shortAction; 
        }

        public DelegateHandler(Action<object, IServiceLocator> fullAction, object key, DispatchMode mode, IReadOnlyCollection<Type> messageTypes)
        {
            if (messageTypes.Count == 0)
                throw new Exception("Empty list of subscribed messages for delegate handler. Should be at least one message.");

            _fullAction = fullAction ?? throw new ArgumentNullException(nameof(fullAction));
            _messageTypes = new List<Type>(messageTypes);
            _dispatchMode = mode;

            // If key wasn't specified, use delegate instance as key.
            _key = key ?? fullAction;
        }

        /// <summary>
        /// Name of the handler. Should show human readable name of handler. Can be not unique.
        /// </summary>
        public string Name => "Delegate Handler";

        /// <summary>
        /// Unique key of the handler. Use this property to uniquily identify this handler.
        /// </summary>
        public object Key => _shortAction;

        /// <summary>
        /// List of types this handler subscribed on
        /// </summary>
        public IEnumerable<Type> Subscriptions => _messageTypes;

        /// <summary>
        /// Create executor 
        /// </summary>
        public void Execute(object message, IServiceLocator serviceLocator)
        {
            if (_shortAction != null)
                _shortAction(message);
            else
                _fullAction(message, serviceLocator);
        }
    }
}