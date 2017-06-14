using System;

namespace Paralect.Core.ServiceBus
{
    public class TransportMessage
    {
        /// <summary>
        /// MessageId
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Message Name
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// Native message format which understood by underlying queue system
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// Message type
        /// </summary>
        public TransportMessageType MessageType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public TransportMessage(object message) : this(message, Guid.NewGuid().ToString(), "UnnamedMessage", TransportMessageType.Normal) {}
        public TransportMessage(object message, string messageId, string messageName, TransportMessageType messageType)
        {
            Message = message;
            MessageId = messageId;
            MessageName = messageName;
            MessageType = messageType;
        }
    }
}
