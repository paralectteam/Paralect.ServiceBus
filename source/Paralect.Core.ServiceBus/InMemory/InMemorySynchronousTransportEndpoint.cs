using System;

namespace Paralect.Core.ServiceBus.InMemory
{
    public class InMemorySynchronousTransportEndpoint : ITransportEndpoint, ITransportEndpointObserver
    {
        private readonly ITransport _transport;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InMemorySynchronousTransportEndpoint(TransportEndpointAddress name, ITransport transport)
        {
            Name = name;
            _transport = transport;
        }

        public void Dispose()
        {
            
        }

        public TransportEndpointAddress Name { get; }

        public event Action<ITransportEndpointObserver> ObserverStarted;
        public event Action<ITransportEndpointObserver> ObserverStopped;
        public event Action<TransportMessage, ITransportEndpointObserver> MessageReceived;

        ITransport ITransportEndpointObserver.Transport => _transport;

        public void Start()
        {
        }

        public void Wait()
        {
        }

        ITransport ITransportEndpoint.Transport => _transport;

        public void Purge()
        {
        }

        public void Send(TransportMessage message)
        {
            var received = MessageReceived;

            received?.Invoke(message, this);
        }

        public TransportMessage Receive(TimeSpan timeout)
        {
            throw new InvalidOperationException("You cannot call Receive() method on Synchronous Queue");
        }
    }
}