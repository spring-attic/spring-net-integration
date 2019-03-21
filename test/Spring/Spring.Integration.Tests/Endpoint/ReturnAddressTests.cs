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
using Spring.Context.Support;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Endpoint
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class ReturnAddressTests
    {
        [Test]
        public void ReturnAddressFallbackWithChannelReference()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel3 = (IMessageChannel) context.GetObject("channel3");
            IPollableChannel channel5 = (IPollableChannel) context.GetObject("channel5");
            ((AbstractApplicationContext) context).Start();
            IMessage message = MessageBuilder.WithPayload("*").SetReplyChannel(channel5).Build();
            channel3.Send(message);
            IMessage response = channel5.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("**"));
        }

        [Test]
        public void ReturnAddressFallbackWithChannelName()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel3 = (IMessageChannel) context.GetObject("channel3");
            IPollableChannel channel5 = (IPollableChannel) context.GetObject("channel5");
            ((AbstractApplicationContext) context).Start();
            IMessage message = MessageBuilder.WithPayload("*").SetReplyChannelName("channel5").Build();
            channel3.Send(message);
            IMessage response = channel5.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("**"));
        }

        [Test]
        public void ReturnAddressWithChannelReferenceAfterMultipleEndpoints()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel1 = (IMessageChannel) context.GetObject("channel1");
            IPollableChannel replyChannel = (IPollableChannel) context.GetObject("replyChannel");
            ((AbstractApplicationContext) context).Start();
            IMessage message = MessageBuilder.WithPayload("*").SetReplyChannel(replyChannel).Build();
            channel1.Send(message);
            IMessage response = replyChannel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("********"));
            IPollableChannel channel2 = (IPollableChannel) context.GetObject("channel2");
            Assert.IsNull(channel2.Receive(TimeSpan.Zero));
        }

        [Test]
        public void ReturnAddressWithChannelNameAfterMultipleEndpoints()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel1 = (IMessageChannel) context.GetObject("channel1");
            IPollableChannel replyChannel = (IPollableChannel) context.GetObject("replyChannel");
            ((AbstractApplicationContext) context).Start();
            IMessage message = MessageBuilder.WithPayload("*").SetReplyChannelName("replyChannel").Build();
            channel1.Send(message);
            IMessage response = replyChannel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("********"));
            IPollableChannel channel2 = (IPollableChannel) context.GetObject("channel2");
            Assert.IsNull(channel2.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (ChannelResolutionException))]
        public void ReturnAddressFallbackButNotAvailable()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel3 = (IMessageChannel) context.GetObject("channel3");
            ((AbstractApplicationContext) context).Start();
            StringMessage message = new StringMessage("*");
            channel3.Send(message);
        }

        [Test]
        public void OutputChannelWithNoReturnAddress()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel4 = (IMessageChannel) context.GetObject("channel4");
            IPollableChannel replyChannel = (IPollableChannel) context.GetObject("replyChannel");
            ((AbstractApplicationContext) context).Start();
            StringMessage message = new StringMessage("*");
            channel4.Send(message);
            IMessage response = replyChannel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("**"));
        }

        [Test]
        public void OutputChannelTakesPrecedence()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\returnAddressTests.xml");
            IMessageChannel channel4 = (IMessageChannel) context.GetObject("channel4");
            IPollableChannel replyChannel = (IPollableChannel) context.GetObject("replyChannel");
            ((AbstractApplicationContext) context).Start();
            IMessage message = MessageBuilder.WithPayload("*").SetReplyChannelName("channel5").Build();
            channel4.Send(message);
            IMessage response = replyChannel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(response);
            Assert.That(response.Payload, Is.EqualTo("**"));
            IPollableChannel channel5 = (IPollableChannel) context.GetObject("channel5");
            Assert.IsNull(channel5.Receive(TimeSpan.Zero));
        }
    }
}