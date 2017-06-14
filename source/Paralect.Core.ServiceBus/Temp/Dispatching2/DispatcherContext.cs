using Microsoft.Practices.ServiceLocation;

namespace Paralect.Core.ServiceBus.Temp.Dispatching2
{
    public class DispatcherContext
    {
        public IServiceLocator ServiceLocator { get; set; }

        /// <summary>
        /// Breaks invocations
        /// </summary>
        public void Break()
        {
            
        }

        /// <summary>
        /// This is default behavier
        /// </summary>
        public void Continue()
        {
            
        }

        public void Dispatch()
        {
            
        }
    }
}