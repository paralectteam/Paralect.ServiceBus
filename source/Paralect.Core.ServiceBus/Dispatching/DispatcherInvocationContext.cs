using System;

namespace Paralect.Core.ServiceBus.Dispatching
{
    public class DispatcherInvocationContext
    {
        private readonly Dispatcher _dispatcher;
        private readonly object _handler;

        public object Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DispatcherInvocationContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DispatcherInvocationContext(Dispatcher dispatcher, Object handler, Object message)
        {
            _dispatcher = dispatcher;
            _handler = handler;
            Message = message;
        }

        public virtual void Invoke()
        {
            _dispatcher.InvokeDynamic(_handler, Message);
        }
    }

    public class DispatcherInterceptorContext : DispatcherInvocationContext
    {
        private readonly IMessageHandlerInterceptor _interceptor;
        private readonly DispatcherInvocationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DispatcherInterceptorContext(IMessageHandlerInterceptor interceptor, DispatcherInvocationContext context)
        {
            _interceptor = interceptor;
            _context = context;
        }

        public override void Invoke()
        {
            _interceptor.Intercept(_context);
        }
    }
}