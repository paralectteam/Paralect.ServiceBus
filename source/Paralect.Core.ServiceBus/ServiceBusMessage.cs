using System;

namespace Paralect.Core.ServiceBus
{
    public class ServiceBusMessage
    {
        public string SentFromComputerName { get; set; }
        public string SentFromQueueName { get; set; }
        public object[] Messages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ServiceBusMessage(params object[] messages)
        {
            Messages = messages;
        }
    }
}





