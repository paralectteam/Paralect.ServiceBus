using System;
using System.Collections.Generic;

namespace Paralect.Core.ServiceBus
{
    /// <summary>
    /// Static 
    /// </summary>
    public static class TransportRegistry
    {
        private static readonly Dictionary<string, ITransport> Map = new Dictionary<string, ITransport>();

        public static void Register(TransportEndpointAddress transportEndpointAddress, ITransport transport)
        {
            if (transport == null)
                return;

            Map[transportEndpointAddress.GetFriendlyName()] = transport;
        }

        public static ITransport GetQueueProvider(TransportEndpointAddress transportEndpointAddress)
        {
            return !Map.TryGetValue(transportEndpointAddress.GetFriendlyName(), out ITransport provider) ? null : provider;
        }
    }
}