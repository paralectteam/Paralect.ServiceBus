using System;

namespace Paralect.Core.ServiceBus.InMemory
{
    public class InMemorySynchronousTransport : InMemoryTransport
    {
        /// <summary>
        /// Create new observer
        /// </summary>
        public override ITransportEndpointObserver CreateObserver(TransportEndpointAddress transportEndpointAddress)
        {
            if (!Queues.ContainsKey(transportEndpointAddress.GetFriendlyName()))
                throw new Exception($"There is no queue with name {transportEndpointAddress.GetFriendlyName()}.");

            return (ITransportEndpointObserver) Queues[transportEndpointAddress.GetFriendlyName()];
        }

        /// <summary>
        /// Create queue
        /// </summary>
        public override void CreateEndpoint(TransportEndpointAddress transportEndpointAddress)
        {
            var queue = new InMemorySynchronousTransportEndpoint(transportEndpointAddress, this);
            Queues[transportEndpointAddress.GetFriendlyName()] = queue;            
        }
    }
}