using System;
using System.Reflection;
using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class Dispatcher : IDispatcher
    {
        /// <summary>
        /// Service Locator that is used to create handlers
        /// </summary>
        private readonly IServiceLocator _serviceLocator;

        /// <summary>
        /// Registry of all registered handlers
        /// </summary>
//        private readonly DispatcherHandlerRegistry _registry;
        private readonly IHandlerRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Dispatcher(DispatcherConfiguration configuration)
        {
            _registry = configuration.Builder.BuildHandlerRegistry();
            _serviceLocator = configuration.ServiceLocator ?? throw new ArgumentException("Service Locator is not registered for distributor.");
        }

        /// <summary>
        /// Factory method
        /// </summary>
        public static Dispatcher Create(Func<DispatcherConfiguration, DispatcherConfiguration> configurationAction)
        {
            var config = new DispatcherConfiguration();
            configurationAction(config);
            return new Dispatcher(config);
        }

        public void Dispatch(object message)
        {
            var type = message.GetType();
            var handers = _registry.GetHandlers(type);

            foreach (var handler in handers)
            {
                handler.Execute(message, _serviceLocator);
            }
        }

        public void InvokeDynamic(object handler, object message)
        {
            dynamic dynamicHandler = handler;
            dynamic dynamicMessage = message;

            dynamicHandler.Handle(dynamicMessage);
        }

        public void InvokeByReflection(object handler, object message)
        {
            var methodInfo = handler.GetType().GetMethod("Handle", new[] { message.GetType() });
            methodInfo.Invoke(handler, new [] {message });
        }
    }
}
