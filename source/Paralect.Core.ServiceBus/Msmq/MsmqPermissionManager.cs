using System;
using System.Messaging;
using System.Security.Principal;

namespace Paralect.Core.ServiceBus.Msmq
{
    public class MsmqPermissionManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string LocalAdministratorsGroupName = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)).ToString();
        private static readonly string LocalEveryoneGroupName = new SecurityIdentifier(WellKnownSidType.WorldSid, null).Translate(typeof(NTAccount)).ToString();
        private static readonly string LocalAnonymousLogonName = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null).Translate(typeof(NTAccount)).ToString();

        /// <summary>
        /// Sets default permissions for queue.
        /// </summary>
        public static void SetPermissionsForQueue(MessageQueue q, string account)
        {
            try
            {
                q.SetPermissions(LocalAdministratorsGroupName, MessageQueueAccessRights.FullControl, AccessControlEntryType.Allow);
                q.SetPermissions(LocalEveryoneGroupName, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow);
                q.SetPermissions(LocalAnonymousLogonName, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow);

                q.SetPermissions(account, MessageQueueAccessRights.WriteMessage, AccessControlEntryType.Allow);
                q.SetPermissions(account, MessageQueueAccessRights.ReceiveMessage, AccessControlEntryType.Allow);
                q.SetPermissions(account, MessageQueueAccessRights.PeekMessage, AccessControlEntryType.Allow);
            }
            catch (Exception ex)
            {
                var message =
                    $"Access to MSMQ queue '{q.Path}' is denied. Please set permission for this queue to be accessable for '{account}' account.";

                Logger.Error(ex, message);
                throw new Exception(message, ex);
            }
        } 
    }
}
