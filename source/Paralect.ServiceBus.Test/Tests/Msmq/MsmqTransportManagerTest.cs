using System;
using NUnit.Framework;
using Paralect.ServiceBus.Msmq;

namespace Paralect.ServiceBus.Test.Tests.Msmq
{
    [TestFixture]
    public class MsmqTransportManagerTest
    {
        [Test]
        public void JustCreation()
        {
            Helper.CreateQueue((queue, manager) =>
            {
                // nothing here
            });
        }

        [Test]
        public void ExistCheck()
        {
            Helper.CreateQueue((queue, manager) =>
            {
                var exists = manager.Exists(queue);
                Assert.AreEqual(exists, true);
            });
        }

        [Test]
        public void NotExistCheck()
        {
            var name = new QueueName(Guid.NewGuid().ToString());
            var manager = new MsmqTransportManager();
            var exists = manager.Exists(name);
            Assert.AreEqual(exists, false);
        }


    }
}