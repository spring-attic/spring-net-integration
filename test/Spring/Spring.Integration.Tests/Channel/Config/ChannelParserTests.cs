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
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Dispatcher;
using Spring.Integration.Message;
using Spring.Integration.Message.Generic;
using Spring.Integration.Tests.Config;
using Spring.Integration.Tests.Util;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Threading;

#endregion

namespace Spring.Integration.Tests.Channel.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class ChannelParserTests
    {
        [Test, ExpectedException(typeof (ObjectDefinitionStoreException))]
        public void TestChannelWithoutId()
        {
            TestUtils.GetContext(@"Channel\Config\ChannelWithoutId.xml");
        }

        [Test]
        public void TestChannelWithCapacity()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");

            IMessageChannel channel = (IMessageChannel) ctx.GetObject("capacityChannel");
            for (int i = 0; i < 10; i++)
            {
                bool result = channel.Send(new Message<string>("test"), TimeSpan.FromMilliseconds(10));
                Assert.IsTrue(result);
            }
            Assert.IsFalse(channel.Send(new Message<string>("test"), TimeSpan.FromMilliseconds(3)));
        }

        [Test]
        public void TestDirectChannelByDefault()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("defaultChannel");
            Assert.That(channel.GetType(), Is.EqualTo(typeof (DirectChannel)));
        }

        [Test]
        public void TestPublishSubscribeChannel()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("publishSubscribeChannel");
            Assert.That(channel.GetType(), Is.EqualTo(typeof (PublishSubscribeChannel)));
        }

        [Test]
        public void TestPublishSubscribeChannelWithTaskExecutorReference()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("publishSubscribeChannelWithTaskExecutorRef");
            Assert.That(channel.GetType(), Is.EqualTo(typeof (PublishSubscribeChannel)));

            IMessageDispatcher dispatcher = TestUtils.GetFieldValue(channel, "_dispatcher") as IMessageDispatcher;
            Assert.IsNotNull(dispatcher);

            IExecutor dispatcherTaskExecutor = TestUtils.GetFieldValue(dispatcher, "_taskExecutor") as IExecutor;
            Assert.IsNotNull(dispatcherTaskExecutor);

            Assert.That(dispatcherTaskExecutor.GetType(), Is.EqualTo(typeof (ErrorHandlingTaskExecutor)));

            IExecutor innerExecutor = TestUtils.GetFieldValue(dispatcherTaskExecutor, "_taskExecutor") as IExecutor;
            object taskExecutorObject = ctx.GetObject("taskExecutor");
            Assert.That(taskExecutorObject, Is.EqualTo(innerExecutor));
        }

        [Test]
        public void TestDatatypeChannelWithCorrectType()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("integerChannel");
            Assert.IsTrue(channel.Send(new Message<int>(123)));
        }

        [Test, ExpectedException(typeof (MessageDeliveryException))]
        public void TestDatatypeChannelWithIncorrectType()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("integerChannel");
            channel.Send(new StringMessage("incorrect type"));
        }

        [Test, Ignore("TODO: int is not assignable to double")]
        public void TestDatatypeChannelWithAssignableSubTypes()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("numberChannel");
            Assert.IsTrue(channel.Send(new Message<int>(123)));
            Assert.IsTrue(channel.Send(new Message<double>(123.45)));
        }

        [Test]
        public void TestMultipleDatatypeChannelWithCorrectTypes()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("stringOrNumberChannel");
            Assert.IsTrue(channel.Send(new Message<double>(123.45)));
            Assert.IsTrue(channel.Send(new StringMessage("accepted type")));
        }

        [Test, ExpectedException(typeof (MessageDeliveryException))]
        public void TestMultipleDatatypeChannelWithIncorrectType()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelParserTests.xml");
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("stringOrNumberChannel");
            channel.Send(new Message<bool>(true));
        }

        [Test]
        public void TestChannelInteceptorRef()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelInterceptorParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("channelWithInterceptorRef");
            TestChannelInterceptor interceptor = (TestChannelInterceptor) ctx.GetObject("interceptor");
            Assert.That(interceptor.SendCount, Is.EqualTo(0));
            channel.Send(new StringMessage("test"));
            Assert.That(interceptor.SendCount, Is.EqualTo(1));
            Assert.That(interceptor.ReceiveCount, Is.EqualTo(0));
            channel.Receive();
            Assert.That(interceptor.ReceiveCount, Is.EqualTo(1));
        }

        [Test]
        public void TestChannelInteceptorInnerBean()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\ChannelInterceptorParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("channelWithInterceptorInnerBean");
            channel.Send(new StringMessage("test"));
            IMessage transformed = channel.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.That(transformed.Payload, Is.EqualTo("TEST"));
        }

        [Test]
        public void TestPriorityChannelWithDefaultComparator()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\PriorityChannelParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("priorityChannelWithDefaultComparator");
            IMessage lowPriorityMessage = MessageBuilder.WithPayload("low").SetPriority(MessagePriority.LOW).Build();
            IMessage midPriorityMessage = MessageBuilder.WithPayload("mid").SetPriority(MessagePriority.NORMAL).Build();
            IMessage highPriorityMessage = MessageBuilder.WithPayload("high").SetPriority(MessagePriority.HIGH).Build();
            channel.Send(lowPriorityMessage);
            channel.Send(highPriorityMessage);
            channel.Send(midPriorityMessage);
            IMessage reply1 = channel.Receive(TimeSpan.Zero);
            IMessage reply2 = channel.Receive(TimeSpan.Zero);
            IMessage reply3 = channel.Receive(TimeSpan.Zero);
            Assert.That(reply1.Payload, Is.EqualTo("high"));
            Assert.That(reply2.Payload, Is.EqualTo("mid"));
            Assert.That(reply3.Payload, Is.EqualTo("low"));
        }

        [Test]
        public void TestPriorityChannelWithCustomComparator()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\PriorityChannelParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("priorityChannelWithCustomComparator");
            channel.Send(new StringMessage("C"));
            channel.Send(new StringMessage("A"));
            channel.Send(new StringMessage("D"));
            channel.Send(new StringMessage("B"));
            IMessage reply1 = channel.Receive(TimeSpan.Zero);
            IMessage reply2 = channel.Receive(TimeSpan.Zero);
            IMessage reply3 = channel.Receive(TimeSpan.Zero);
            IMessage reply4 = channel.Receive(TimeSpan.Zero);
            Assert.That(reply1.Payload, Is.EqualTo("A"));
            Assert.That(reply2.Payload, Is.EqualTo("B"));
            Assert.That(reply3.Payload, Is.EqualTo("C"));
            Assert.That(reply4.Payload, Is.EqualTo("D"));
        }

        [Test]
        public void TestPriorityChannelWithIntegerDatatypeEnforced()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\PriorityChannelParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("integerOnlyPriorityChannel");
            channel.Send(new Message<int>(3));
            channel.Send(new Message<int>(2));
            channel.Send(new Message<int>(1));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo(1));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo(2));
            Assert.That(channel.Receive(TimeSpan.Zero).Payload, Is.EqualTo(3));
        }

        [Test, ExpectedException(typeof (MessageDeliveryException))]
        public void TestPriorityChannelWithIntegerDatatypeEnforced2()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Channel\Config\PriorityChannelParserTests.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("integerOnlyPriorityChannel");
            channel.Send(new StringMessage("wrong type"));
        }
    }
}