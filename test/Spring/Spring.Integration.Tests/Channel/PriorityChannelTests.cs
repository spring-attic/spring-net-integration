#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class PriorityChannelTests
    {
        [Test]
        public void testCapacityEnforced()
        {
            PriorityChannel channel = new PriorityChannel(3);
            Assert.IsTrue(channel.Send(new StringMessage("test1"), TimeSpan.Zero));
            Assert.IsTrue(channel.Send(new StringMessage("test2"), TimeSpan.Zero));
            Assert.IsTrue(channel.Send(new StringMessage("test3"), TimeSpan.Zero));
            Assert.IsFalse(channel.Send(new StringMessage("test4"), TimeSpan.Zero));
            channel.Receive(TimeSpan.Zero);
            Assert.IsTrue(channel.Send(new StringMessage("test5")));
        }

        [Test]
        public void testDefaultComparator()
        {
            PriorityChannel channel = new PriorityChannel(5);
            IMessage priority1 = CreatePriorityMessage(MessagePriority.HIGHEST);
            IMessage priority2 = CreatePriorityMessage(MessagePriority.HIGH);
            IMessage priority3 = CreatePriorityMessage(MessagePriority.NORMAL);
            IMessage priority4 = CreatePriorityMessage(MessagePriority.LOW);
            IMessage priority5 = CreatePriorityMessage(MessagePriority.LOWEST);
            channel.Send(priority4);
            channel.Send(priority3);
            channel.Send(priority5);
            channel.Send(priority1);
            channel.Send(priority2);
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-HIGHEST"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-HIGH"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-NORMAL"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-LOW"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-LOWEST"));
        }

        [Test]
        public void testCustomComparator()
        {
            PriorityChannel channel = new PriorityChannel(5, new StringPayloadComparator());
            IMessage messageA = new StringMessage("A");
            IMessage messageB = new StringMessage("B");
            IMessage messageC = new StringMessage("C");
            IMessage messageD = new StringMessage("D");
            IMessage messageE = new StringMessage("E");
            channel.Send(messageC);
            channel.Send(messageA);
            channel.Send(messageE);
            channel.Send(messageD);
            channel.Send(messageB);
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("A"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("B"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("C"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("D"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("E"));
        }

        [Test]
        public void testNullPriorityIsConsideredNormal()
        {
            PriorityChannel channel = new PriorityChannel(5);
            IMessage highPriority = CreatePriorityMessage(MessagePriority.HIGH);
            IMessage lowPriority = CreatePriorityMessage(MessagePriority.LOW);
            IMessage nullPriority = new StringMessage("test-NULL");
            channel.Send(lowPriority);
            channel.Send(highPriority);
            channel.Send(nullPriority);
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-HIGH"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-NULL"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-LOW"));
        }

        [Test]
        public void testUnboundedCapacity()
        {
            PriorityChannel channel = new PriorityChannel();
            IMessage highPriority = CreatePriorityMessage(MessagePriority.HIGH);
            IMessage lowPriority = CreatePriorityMessage(MessagePriority.LOW);
            IMessage nullPriority = new StringMessage("test-NULL");
            channel.Send(lowPriority);
            channel.Send(highPriority);
            channel.Send(nullPriority);
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-HIGH"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-NULL"));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo("test-LOW"));
        }

        private static IMessage CreatePriorityMessage(MessagePriority priority)
        {
            return MessageBuilder.WithPayload("test-" + priority).SetPriority(priority).Build();
        }


        public class StringPayloadComparator : IComparer<IMessage>
        {
            public int Compare(IMessage message1, IMessage message2)
            {
                string s1 = (string) message1.Payload;
                string s2 = (string) message2.Payload;
                return s1.CompareTo(s2);
            }
        }
    }
}