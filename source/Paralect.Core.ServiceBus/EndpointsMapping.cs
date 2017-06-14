using System;
using System.Collections.Generic;
using System.Linq;

namespace Paralect.Core.ServiceBus
{
    public class EndpointsMapping
    {
        public List<EndpointDirection> Endpoints { get; } = new List<EndpointDirection>();

        public void Map(Func<Type, bool> typeChecker, string queueName, ITransport transport)
        {
            var endpoint = new EndpointDirection
            {
                Address = new TransportEndpointAddress(queueName),
                TypeChecker = typeChecker,
                Transport = transport
            };

            Endpoints.Add(endpoint);
        }

        public List<EndpointDirection> GetEndpoints(Type type)
        {
            return Endpoints.Where(endpoint => endpoint.TypeChecker(type)).ToList();
        }
    }

    public class EndpointDirection
    {
        public Func<Type, bool> TypeChecker { get; set; }
        public TransportEndpointAddress Address { get; set; }

        /// <summary>
        /// Can be null
        /// </summary>
        public ITransport Transport { get; set; }
    }
}
