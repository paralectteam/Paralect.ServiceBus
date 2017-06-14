using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paralect.Core.ServiceBus.Dispatching
{
    public class DispatcherHandlerRegistry
    {
        public Type MarkerInterface { get; set; }

        /// <summary>
        /// Message type -> List of handlers type
        /// </summary>
        private readonly Dictionary<Type, List<Type>> _subscription = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Message interceptors
        /// </summary>
        private readonly List<Type> _interceptors = new List<Type>();

        /// <summary>
        /// Message interceptors
        /// </summary>
        public List<Type> Interceptors => _interceptors;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DispatcherHandlerRegistry()
        {
            MarkerInterface = typeof(IMessageHandler<>);
        }

        /// <summary>
        /// Register all handlers in assembly (you can register handlers that optionally belongs to specified namespaces)
        /// </summary>
        public void Register(Assembly assembly, string[] namespaces)
        {
            var searchTarget = MarkerInterface;

            var assemblySubscriptions = assembly
                .GetTypes()
                .Where(t => BelongToNamespaces(t, namespaces))
                .SelectMany(t => t.GetInterfaces()
                                    .Where(i => i.IsGenericType
                                    && (i.GetGenericTypeDefinition() == searchTarget)
                                    && !i.ContainsGenericParameters),
                            (t, i) => new { Key = i.GetGenericArguments()[0], Value = t })
                .GroupBy(x => x.Key, x => x.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var key in assemblySubscriptions.Keys)
            {
                var types = assemblySubscriptions[key];

                if (!_subscription.ContainsKey(key))
                    _subscription[key] = new List<Type>();

                foreach (var type in types)
                {
                    // Skip handler already registered for that message type, skip it!
                    if (!_subscription[key].Contains(type))
                        _subscription[key].Add(type);
                }
            }
        }

        public void InsureOrderOfHandlers(List<Type> order)
        {
            if (order.Count <= 1)
                return;

            foreach (var type in _subscription.Keys)
            {
                var handlerTypes = _subscription[type];
                SortInPlace(handlerTypes, order);
            }
        }

        public void SortInPlace(List<Type> list, List<Type> orders)
        {
            if (orders.Count <= 1)
                return;

            list.Sort((type1, type2) =>
            {
                var first = orders.IndexOf(type1);
                var second = orders.IndexOf(type2);

                if (first == -1 && second == -1)
                    return 0;

                if (first == -1 && second != -1)
                    return 1;

                if (first != -1 && second == -1)
                    return -1;

                return (first < second) ? -1 : 1;
            });
        }

        public void AddInterceptor(Type type)
        {
            if (!typeof(IMessageHandlerInterceptor).IsAssignableFrom(type))
                throw new Exception($"Interceptor {type.FullName} must implement IMessageHandlerInterceptor");

            if (_interceptors.Contains(type))
                throw new Exception($"Interceptor {type.FullName} already registered");

            _interceptors.Add(type);
        }

        private bool BelongToNamespaces(Type type, string[] namespaces)
        {
            // if no namespaces specified - then type belong to any namespace
            return namespaces.Length == 0 || namespaces.Any(ns => type.FullName.StartsWith(ns));
        }

        public List<Type> GetHandlersType(Type messageType)
        {
            var errorMessage = $"Handler for type {messageType.FullName} doesn't found.";

            if (!_subscription.ContainsKey(messageType))
                return new List<Type>();

            var handlers = _subscription[messageType];

            if (handlers.Count < 1)
                throw new Exception(errorMessage);

            return handlers;
        }
    }
}
