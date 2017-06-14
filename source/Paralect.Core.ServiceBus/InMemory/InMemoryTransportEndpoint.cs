using System;
using Paralect.Core.ServiceBus.Exceptions;

namespace Paralect.Core.ServiceBus.InMemory
{
    public class InMemoryTransportEndpoint : ITransportEndpoint
    {
        private readonly BlockingQueue<TransportMessage> _messages = new BlockingQueue<TransportMessage>();

        /// <summary>
        /// Logger instance (In future we should  go away from NLog dependency)
        /// </summary>
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InMemoryTransportEndpoint(TransportEndpointAddress name, ITransport transport)
        {
            Name = name;
            Transport = transport;
        }

        public void Dispose()
        {
            
        }

        public TransportEndpointAddress Name { get; }

        public ITransport Transport { get; }

        public void Purge()
        {
            _messages.Clear();
        }

        public void Send(TransportMessage message)
        {
            _messages.Enqueue(message);
        }

        public TransportMessage Receive(TimeSpan timeout)
        {
            TransportMessage message;
            var result = _messages.TryDequeue(out message, (int) timeout.TotalMilliseconds);

            if (!result)
                throw new TransportTimeoutException("Timeout when receiving message");

            return message;
        }
    }
}