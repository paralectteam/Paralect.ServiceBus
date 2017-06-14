using System;
using System.Threading;
using Paralect.Core.ServiceBus.Exceptions;
using Paralect.Core.ServiceBus.Utils;

namespace Paralect.Core.ServiceBus
{
    /// <summary>
    /// Observer that use one thread 
    /// </summary>
    public class SingleThreadTransportEndpointObserver : ITransportEndpointObserver
    {
        private readonly TransportEndpointAddress _transportEndpointAddress;
        private readonly string _threadName;
        private Thread _observerThread;
        private bool _continue;

        public event Action<ITransportEndpointObserver> ObserverStarted;
        public event Action<ITransportEndpointObserver> ObserverStopped;
        public event Action<TransportMessage, ITransportEndpointObserver> MessageReceived;

        private readonly string _shutdownMessageId = Guid.NewGuid().ToString();

        /// <summary>
        /// Log
        /// </summary>
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public ITransport Transport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SingleThreadTransportEndpointObserver(ITransport transport, TransportEndpointAddress transportEndpointAddress, String threadName = null)
        {
            Transport = transport;
            _transportEndpointAddress = transportEndpointAddress;
            _threadName = threadName;
        }

        public void Start()
        {
            _continue = true;
            _observerThread = new Thread(QueueObserverThread)
            {
                Name = _threadName ??
                       $"Transport Observer thread for queue {_transportEndpointAddress.GetFriendlyName()}",
                IsBackground = true,
            };
            _observerThread.Start();            
        }

        protected void QueueObserverThread(object state)
        {
            // Only one instance of ServiceBus should listen to a particular queue
            var mutexName = $"Paralect.ServiceBus.{_transportEndpointAddress.GetFriendlyName()}";

            MutexFactory.LockByMutex(mutexName, () =>
            {
                var started = ObserverStarted;
                started?.Invoke(this);

                Log.Info("Paralect Service [{0}] bus started and listen to the {1} queue...", "_configuration.Name", _transportEndpointAddress.GetFriendlyName());
                Observe();
            });
        }

        private void Observe()
        {
            try
            {
                var queue = Transport.OpenEndpoint(_transportEndpointAddress);

                while (_continue)
                {
                    try
                    {
                        var transportMessage = queue.Receive(TimeSpan.FromDays(10));
                        
                        if (transportMessage.MessageType == TransportMessageType.Shutdown)
                        {
                            if (transportMessage.MessageId == _shutdownMessageId)
                                break;

                            continue;
                        }

                        var received = MessageReceived;
                        received?.Invoke(transportMessage, this);
                    }
                    catch (TransportTimeoutException)
                    {
                        continue;
                    }
                }
                
            }
            catch (ThreadAbortException abortException)
            {
                var wrapper = new Exception("Thread listener was aborted in Service Bus [_serviceBusName]", abortException);
                Log.Fatal(wrapper, "");
            }
            catch (Exception ex)
            {
                var wrapper = new Exception("Fatal exception in Service Bus [_serviceBusName]", ex);
                Log.Fatal(wrapper, "");
                throw wrapper;
            }
        }

        public void Wait()
        {
            SendStopMessages();
            _observerThread.Join();
        }

        public void Dispose()
        {
            _continue = false;
            SendStopMessages();
            _observerThread.Join();
        }

        private void SendStopMessages()
        {
            Transport
                .OpenEndpoint(_transportEndpointAddress)
                .Send(new TransportMessage(null, _shutdownMessageId, "Shutdown", TransportMessageType.Shutdown));
        }
    }
}