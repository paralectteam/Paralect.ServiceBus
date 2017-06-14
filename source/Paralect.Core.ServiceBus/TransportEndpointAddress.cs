using System;

namespace Paralect.Core.ServiceBus
{
    public class TransportEndpointAddress
    {
        public string Name { get; set; }
        public string ComputerName { get; set; }

        /// <summary>
        /// Should be in the format:
        /// QueueName@ComputerName
        /// </summary>
        public TransportEndpointAddress(String path)
        {
            var parts = path.Split('@');
            Name = parts[0];
            ComputerName = (parts.Length == 2) ? parts[1] : Environment.MachineName;
        }

        public string GetFormatName()
        {
            // MSMQ format name:
            // FormatName:Direct=OS:machinename\\private$\\queue
            return $@"FormatName:DIRECT=OS:{ComputerName}\private$\{Name}";
        }

        public string GetLocalName()
        {
            return $@"{"."}\private$\{Name}";
        }

        public string GetFriendlyName()
        {
            return $"{Name}@{ComputerName}";
        }
    }

    public interface IEndpointAddress
    {
        
    }
}
