using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public interface IHandler
    {
        /// <summary>
        /// Name of the handler. Should show human readable name of handler. Can be not unique.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Unique key of the handler. Use this property to uniquily identify this handler.
        /// </summary>
        object Key { get; }

        /// <summary>
        /// List of types this handler subscribed on
        /// </summary>
        IEnumerable<Type> Subscriptions { get; }

        /// <summary>
        /// Execute handler with specified message
        /// </summary>
        void Execute(object message, IServiceLocator serviceLocator);
    }
}