using System;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace Paralect.Core.ServiceBus.Utils
{
    public static class MutexFactory
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static Mutex CreateMutexWithFullControlRights(string name, out bool createdNew)
        {
            var securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var mutexSecurity = new MutexSecurity();
            var rule = new MutexAccessRule(securityIdentifier, MutexRights.FullControl, AccessControlType.Allow);
            mutexSecurity.AddAccessRule(rule);
            return new Mutex(false, name, out createdNew, mutexSecurity);
        }

        public static void LockByMutex(string name, Action action)
        {
            var mutexIsNew = false;
            var queueMutex = CreateMutexWithFullControlRights(name, out mutexIsNew);
            bool owned = false;

            try
            {
                while (!owned)
                {
                    try { owned = queueMutex.WaitOne(-1, false); }
                    // Our main resource (queue) supposed to be always in the valid state. 
                    // That is why we are ignoring AbandonedMutexException.
                    // http://msdn.microsoft.com/en-us/library/system.threading.abandonedmutexexception.aspx
                    catch (AbandonedMutexException ex) { }
                }

                action();
            }
            finally
            {
                if (owned)
                {
                    queueMutex.ReleaseMutex();
                    queueMutex.Close();
                    
                }
            }            
        }
    }
}
