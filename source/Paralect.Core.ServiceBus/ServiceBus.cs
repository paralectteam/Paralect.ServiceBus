using System;
using Paralect.Core.ServiceBus.Dispatching;
using Paralect.Core.ServiceBus.Exceptions;
using Paralect.Core.ServiceBus.Utils;

namespace Paralect.Core.ServiceBus
{
    /// <summary>
    /// Default implementation of IBus 
    /// </summary>
    public class ServiceBus : IServiceBus
    {
        /// <summary>
        /// Configuration settings for ServiceBus
        /// </summary>
        private readonly ServiceBusConfiguration _configuration;
        
        /// <summary>
        /// Endpoint Provider used to send and receive messages
        /// </summary>
        private readonly ITransport _provider;

        /// <summary>
        /// Input query address from which we are receiving messages
        /// </summary>
        private readonly TransportEndpointAddress _inputTransportEndpointAddress;

        /// <summary>
        /// Error queue address (we are sending to error queue messsages that wasn't handled correctly)
        /// </summary>
        private readonly TransportEndpointAddress _errorTransportEndpointAddress;

        /// <summary>
        /// Error endpoint (we are sending to error queue messsages that wasn't handled correctly)
        /// </summary>
        private ITransportEndpoint _errorTransportEndpoint;

        /// <summary>
        /// Last exception that was "produced" by this service bus
        /// </summary>
        private Exception _lastException;

        /// <summary>
        /// Observer of input queue
        /// </summary>
        private ITransportEndpointObserver _transportEndpointObserver;

        /// <summary>
        /// Dispatcher of messages
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// Mapping between endpoints and message type
        /// </summary>
        private readonly EndpointsMapping _endpointMapping;

        /// <summary>
        /// Current status of bus
        /// </summary>
        private ServiceBusStatus _status = ServiceBusStatus.Stopped;

        /// <summary>
        /// Logger
        /// </summary>
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ServiceBus(ServiceBusConfiguration configuration)
        {
            if (configuration.ServiceLocator == null)
                throw new ArgumentException("Service Locator doesn't registered. Use SetServiceLocator() method.");

            if (configuration.InputQueue == null)
                throw new ArgumentException("Input queue not configured. Use SetInputQueue() method.");

            _configuration = configuration;
            _provider = configuration.Transport;
            _inputTransportEndpointAddress = configuration.InputQueue;
            _errorTransportEndpointAddress = configuration.ErrorQueue;
            _endpointMapping = configuration.EndpointsMapping;

            // use container of ServiceBus, if not specified for dispatcher
            if (_configuration.DispatcherConfiguration.ServiceLocator == null)
                _configuration.DispatcherConfiguration.ServiceLocator = configuration.ServiceLocator;

            TransportRegistry.Register(_inputTransportEndpointAddress, _provider);
            TransportRegistry.Register(_errorTransportEndpointAddress, _provider);
        }

        /// <summary>
        /// Run service bus and start input endpoint observing
        /// </summary>
        public void Run()
        {
            // Registration of endpoints
            foreach (var endpoint in _endpointMapping.Endpoints)
            {
                if (TransportRegistry.GetQueueProvider(endpoint.Address) == null)
                    TransportRegistry.Register(endpoint.Address, endpoint.Transport ?? _provider);
            }

            // Check existence of input and error endpoint and create them if needed
            PrepareQueues();

            // Create dispatcher
            _dispatcher = new Dispatcher(_configuration.DispatcherConfiguration);

            // Open error queue
            _errorTransportEndpoint = _provider.OpenEndpoint(_errorTransportEndpointAddress);

            // Create and configure observer of input queue
            _transportEndpointObserver = _provider.CreateObserver(_inputTransportEndpointAddress);
            _transportEndpointObserver.MessageReceived += EndpointObserverMessageReceived;
            _transportEndpointObserver.Start();

            // Set servise bus state into Running state 
            _status = ServiceBusStatus.Running;
        }

        /// <summary>
        /// Handle messages that were received from input endpoint
        /// Method can be called from different threads.
        /// </summary>
        private void EndpointObserverMessageReceived(TransportMessage transportMessage, ITransportEndpointObserver transportEndpointObserver)
        {
            try
            {
                // Translate to Transport message
                var serviceBusMessage = transportEndpointObserver.Transport.TranslateToServiceBusMessage(transportMessage);

                // Ignore transport messages without messages
                if (serviceBusMessage.Messages != null && (serviceBusMessage.Messages == null && serviceBusMessage.Messages.Length < 1))
                    return;

                // Dispatch each message (synchronously)
                if (serviceBusMessage.Messages != null)
                    foreach (var message in serviceBusMessage.Messages)
                    {
                        _dispatcher.Dispatch(message);
                    }
            }
            catch (DispatchingException dispatchingException)
            {
                _lastException = dispatchingException;
                Log.Error(dispatchingException, "Dispatching exception. See logs for more details.");
                _errorTransportEndpoint.Send(transportMessage);
            }
            catch (HandlerException handlerException)
            {
                _lastException = handlerException;
                Log.Error(handlerException, "Message handling failed.");
                _errorTransportEndpoint.Send(transportMessage);
            }
            catch (TransportMessageDeserializationException deserializationException)
            {
                _lastException = deserializationException;
                Log.Error(deserializationException, "Unable to deserialize message #" + transportMessage.MessageId);
                _errorTransportEndpoint.Send(transportMessage);
            }
        }

        /// <summary>
        /// Check existence of input and error endpoint and create them if needed
        /// </summary>
        private void PrepareQueues()
        {
            // Prepare input endpoint
            var inputMutexName =
                $"Paralect.ServiceBus.{_inputTransportEndpointAddress.GetFriendlyName()}.InputQueue";
            if (!_provider.ExistsEndpoint(_inputTransportEndpointAddress))
            {
                MutexFactory.LockByMutex(inputMutexName, () =>
                {
                    if (!_provider.ExistsEndpoint(_inputTransportEndpointAddress))
                        _provider.CreateEndpoint(_inputTransportEndpointAddress);
                });
            }

            _provider.PrepareEndpoint(_inputTransportEndpointAddress);


            // Prepare error endpoint
            var errorMutexName =
                $"Paralect.ServiceBus.{_inputTransportEndpointAddress.GetFriendlyName()}.ErrorQueue";
            if (!_provider.ExistsEndpoint(_errorTransportEndpointAddress))
            {
                MutexFactory.LockByMutex(errorMutexName, () =>
                {
                    if (!_provider.ExistsEndpoint(_errorTransportEndpointAddress))
                        _provider.CreateEndpoint(_errorTransportEndpointAddress);

                });
            }

            _provider.PrepareEndpoint(_errorTransportEndpointAddress);
        }

        /// <summary>
        /// Stop service bus and block thread until bus will be stopped.
        /// Maybe rename it to Stop() ?
        /// </summary>
        public void Wait()
        {
            _transportEndpointObserver.Wait();
            _status = ServiceBusStatus.Stopped;
        }

        /// <summary>
        /// Do cleanup logic and stop service bus
        /// </summary>
        public void Dispose()
        {
            if (_transportEndpointObserver != null)
            {
                _transportEndpointObserver.MessageReceived -= EndpointObserverMessageReceived;

                if (_status != ServiceBusStatus.Stopped)
                    _transportEndpointObserver.Dispose();
            }
        }

        /// <summary>
        /// Send message. Recipient resolved by the object type.
        /// </summary>
        public void Send(params object[] messages)
        {
            // Skip if there is no messages
            if (messages == null || messages.Length < 1)
                return;

            // Create transport message
            var serviceBusMessage =
                new ServiceBusMessage(messages) {SentFromQueueName = _inputTransportEndpointAddress.GetFriendlyName()};

            // Get list of endpoints we need send message to
            var endpoints = _endpointMapping.GetEndpoints(messages[0].GetType());

            foreach (var endpoint in endpoints)
            {
                // Create EndpointMessage from TransportMessage
                var provider = TransportRegistry.GetQueueProvider(endpoint.Address);
                var transportMessage = provider.TranslateToTransportMessage(serviceBusMessage);

                // Send message
                var queue = provider.OpenEndpoint(endpoint.Address);
                queue.Send(transportMessage);
            }
        }

        /// <summary>
        /// Send message to input queue of this service bus instance.
        /// Can be used for "requeueing" of message.
        /// </summary>
        /// <param name="messages"></param>
        public void SendLocal(params object[] messages)
        {
            var serviceBusMessage = new ServiceBusMessage(messages);
            var transportMessage = _provider.TranslateToTransportMessage(serviceBusMessage);
            _errorTransportEndpoint.Send(transportMessage);
        }

        /// <summary>
        /// We are using Send() here. Publishing/subscribing not implemented yet.
        /// </summary>
        public void Publish(object message)
        {
            Send(message);
        }

        /// <summary>
        /// Return last exception or null if no exceptions
        /// </summary>
        public Exception GetLastException()
        {
            return _lastException;
        }

        /// <summary>
        /// Factory method. Create and run service bus
        /// </summary>
        public static IServiceBus Run(Func<ServiceBusConfiguration, ServiceBusConfiguration> configurationAction)
        {
            var config = new ServiceBusConfiguration();
            configurationAction(config);
            var bus = new ServiceBus(config);
            bus.Run();
            return bus;
        }

        /// <summary>
        /// Factory method. Create service bus.
        /// </summary>
        public static IServiceBus Create(Func<ServiceBusConfiguration, ServiceBusConfiguration> configurationAction)
        {
            var config = new ServiceBusConfiguration();
            configurationAction(config);
            return new ServiceBus(config);
        }
    }
}