using System;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        void Dispatch(object message);
    }
}