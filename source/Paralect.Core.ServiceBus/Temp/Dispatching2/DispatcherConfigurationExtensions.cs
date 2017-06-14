using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public static class DispatcherConfigurationExtensions
    {
        public static DispatcherConfiguration SetServiceLocator(this DispatcherConfiguration configuration, IServiceLocator container)
        {
            configuration.ServiceLocator = container;
            return configuration;
        }

        public static DispatcherConfiguration SetNumberOfRetries(this DispatcherConfiguration configuration, int numberOfRetries)
        {
            configuration.NumberOfRetries = numberOfRetries;
            return configuration;
        }


        public static DispatcherConfiguration InsureMessageHandlingOrder<TMessage, THandler1, THandler2>(this DispatcherConfiguration configuration) { return configuration;  }
        public static DispatcherConfiguration InsureMessageHandlingOrder<TMessage, THandler1, THandler2, THandler3>(this DispatcherConfiguration configuration) { return configuration; }
        public static DispatcherConfiguration InsureMessageHandlingOrder<TMessage, THandler1, THandler2, THandler3, THandler4>(this DispatcherConfiguration configuration) { return configuration; }
        public static DispatcherConfiguration InsureMessageHandlingOrder<TMessage, THandler1, THandler2, THandler3, THandler4, THandler5>(this DispatcherConfiguration configuration) { return configuration; }

        public static DispatcherConfiguration InsureHandlingOrder<THandler1, THandler2>(this DispatcherConfiguration configuration) { return configuration; }
        public static DispatcherConfiguration InsureHandlingOrder<THandler1, THandler2, THandler3>(this DispatcherConfiguration configuration) { return configuration; }
        public static DispatcherConfiguration InsureHandlingOrder<THandler1, THandler2, THandler3, THandler4>(this DispatcherConfiguration configuration) { return configuration; }
        public static DispatcherConfiguration InsureHandlingOrder<THandler1, THandler2, THandler3, THandler4, THandler5>(this DispatcherConfiguration configuration) { return configuration; }

        public static DispatcherConfiguration InsureHandlingOrder(this DispatcherConfiguration configuration, params object[] handlersKeys) { return configuration; }
        public static DispatcherConfiguration InsureHandlingOrder(this DispatcherConfiguration configuration, params Type[] handlers) { return configuration; }

        public static DispatcherConfiguration InsureMessageHandlingOrder(this DispatcherConfiguration configuration, Type messageType, params object[] handlers) { return configuration; }
        public static DispatcherConfiguration InsureMessageHandlingOrder(this DispatcherConfiguration configuration, Type messageType, params Type[] handlers) { return configuration; }

        /// <summary>
        /// Register single handler. This is a way to register custom handlers.
        /// </summary>
        public static DispatcherConfiguration RegisterHandler(this DispatcherConfiguration configuration, IHandler handler)
        {
            configuration.Builder.Register(handler);
            return configuration;
        }

        public static DispatcherConfiguration SetHandlerMarkerInterface(this DispatcherConfiguration configuration, Type markerInterface)
        {
            configuration.MessageHandlerMarkerInterface = markerInterface;
            return configuration;
        }

        public static DispatcherConfiguration SetOrder(this DispatcherConfiguration configuration, params Type[] types)
        {
            configuration.Order = types.ToList();
            return configuration;
        }
    }
}