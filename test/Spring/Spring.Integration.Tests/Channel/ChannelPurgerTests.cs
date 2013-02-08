#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
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
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class ChannelPurgerTests
    {
        [Test]
        public void testPurgeAllWithoutSelector()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test1"));
            channel.Send(new StringMessage("test2"));
            channel.Send(new StringMessage("test3"));
            ChannelPurger purger = new ChannelPurger(channel);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(3));
            Assert.IsNull(channel.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testPurgeAllWithSelector()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test1"));
            channel.Send(new StringMessage("test2"));
            channel.Send(new StringMessage("test3"));
            ChannelPurger purger = new ChannelPurger(delegate { return false; }, channel);

            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(3));
            Assert.IsNull(channel.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testPurgeNoneWithSelector()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test1"));
            channel.Send(new StringMessage("test2"));
            channel.Send(new StringMessage("test3"));
            ChannelPurger purger = new ChannelPurger(delegate { return true; }, channel);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(0));
            Assert.IsNotNull(channel.Receive(TimeSpan.Zero));
            Assert.IsNotNull(channel.Receive(TimeSpan.Zero));
            Assert.IsNotNull(channel.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testPurgeSubsetWithSelector()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test1"));
            channel.Send(new StringMessage("test2"));
            channel.Send(new StringMessage("test3"));
            ChannelPurger purger = new ChannelPurger(delegate(IMessage msg) { return (msg.Payload.Equals("test2")); },
                                                     channel);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(2));
            IMessage message = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(message);
            Assert.That(message.Payload, Is.EqualTo("test2"));
            Assert.IsNull(channel.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testMultipleChannelsWithNoSelector()
        {
            QueueChannel channel1 = new QueueChannel();
            QueueChannel channel2 = new QueueChannel();
            channel1.Send(new StringMessage("test1"));
            channel1.Send(new StringMessage("test2"));
            channel2.Send(new StringMessage("test1"));
            channel2.Send(new StringMessage("test2"));
            ChannelPurger purger = new ChannelPurger(channel1, channel2);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(4));
            Assert.IsNull(channel1.Receive(TimeSpan.Zero));
            Assert.IsNull(channel2.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testMultipleChannelsWithSelector()
        {
            QueueChannel channel1 = new QueueChannel();
            QueueChannel channel2 = new QueueChannel();
            channel1.Send(new StringMessage("test1"));
            channel1.Send(new StringMessage("test2"));
            channel1.Send(new StringMessage("test3"));
            channel2.Send(new StringMessage("test1"));
            channel2.Send(new StringMessage("test2"));
            channel2.Send(new StringMessage("test3"));
            ChannelPurger purger = new ChannelPurger(delegate(IMessage msg) { return (msg.Payload.Equals("test2")); },
                                                     channel1, channel2);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(4));
            IMessage message1 = channel1.Receive(TimeSpan.Zero);
            Assert.IsNotNull(message1);
            Assert.That(message1.Payload, Is.EqualTo("test2"));
            Assert.IsNull(channel1.Receive(TimeSpan.Zero));
            IMessage message2 = channel2.Receive(TimeSpan.Zero);
            Assert.IsNotNull(message2);
            Assert.That(message2.Payload, Is.EqualTo("test2"));
            Assert.IsNull(channel2.Receive(TimeSpan.Zero));
        }

        [Test]
        public void testPurgeNoneWithSelectorAndMultipleChannels()
        {
            QueueChannel channel1 = new QueueChannel();
            QueueChannel channel2 = new QueueChannel();
            channel1.Send(new StringMessage("test1"));
            channel1.Send(new StringMessage("test2"));
            channel2.Send(new StringMessage("test1"));
            channel2.Send(new StringMessage("test2"));
            ChannelPurger purger = new ChannelPurger(delegate { return true; }, channel1, channel2);
            IList<IMessage> purgedMessages = purger.Purge();
            Assert.That(purgedMessages.Count, Is.EqualTo(0));
            Assert.IsNotNull(channel1.Receive(TimeSpan.Zero));
            Assert.IsNotNull(channel1.Receive(TimeSpan.Zero));
            Assert.IsNotNull(channel2.Receive(TimeSpan.Zero));
            Assert.IsNotNull(channel2.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void testNullChannel()
        {
            QueueChannel channel = null;
            new ChannelPurger(channel);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void testEmptyChannelArray()
        {
            QueueChannel[] channels = new QueueChannel[0];
            new ChannelPurger(channels);
        }
    }
}