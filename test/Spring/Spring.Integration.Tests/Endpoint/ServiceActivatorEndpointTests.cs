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
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Tests.Channel;

#endregion

namespace Spring.Integration.Tests.Endpoint
{
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class ServiceActivatorEndpointTests
    {
        [Test]
        public void OutputChannel()
        {
            QueueChannel channel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = CreateEndpoint();
            endpoint.OutputChannel = channel;
            IMessage message = MessageBuilder.WithPayload("foo").Build();
            endpoint.HandleMessage(message);
            IMessage reply = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("FOO"));
        }

        [Test]
        public void OutputChannelTakesPrecedence()
        {
            QueueChannel channel1 = new QueueChannel(1);
            QueueChannel channel2 = new QueueChannel(1);
            ServiceActivatingHandler endpoint = CreateEndpoint();
            endpoint.OutputChannel = channel1;
            IMessage message = MessageBuilder.WithPayload("foo").SetReplyChannel(channel2).Build();
            endpoint.HandleMessage(message);
            IMessage reply1 = channel1.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply1);
            Assert.That(reply1.Payload, Is.EqualTo("FOO"));
            IMessage reply2 = channel2.Receive(TimeSpan.Zero);
            Assert.IsNull(reply2);
        }

        [Test]
        public void ReturnAddressHeader()
        {
            QueueChannel channel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = CreateEndpoint();
            IMessage message = MessageBuilder.WithPayload("foo").SetReplyChannel(channel).Build();
            endpoint.HandleMessage(message);
            IMessage reply = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("FOO"));
        }

        [Test]
        public void ReturnAddressHeaderWithChannelName()
        {
            QueueChannel channel = new QueueChannel(1);
            channel.ObjectName = "testChannel";
            TestChannelResolver channelResolver = new TestChannelResolver();
            channelResolver.AddChannel(channel);
            ServiceActivatingHandler endpoint = CreateEndpoint();
            endpoint.ChannelResolver = channelResolver;
            IMessage message = MessageBuilder.WithPayload("foo")
                .SetReplyChannelName("testChannel").Build();
            endpoint.HandleMessage(message);
            IMessage reply = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("FOO"));
        }

        [Test]
        public void DynamicReplyChannel()
        {
            QueueChannel replyChannel1 = new QueueChannel();
            QueueChannel replyChannel2 = new QueueChannel();
            replyChannel2.ObjectName = "replyChannel2";
            Object handler = new TestObject2();
            ServiceActivatingHandler endpoint = new ServiceActivatingHandler(handler, "Handle");
            TestChannelResolver channelResolver = new TestChannelResolver();
            channelResolver.AddChannel(replyChannel2);
            endpoint.ChannelResolver = channelResolver;
            IMessage testMessage1 = MessageBuilder.WithPayload("bar").SetReplyChannel(replyChannel1).Build();
            endpoint.HandleMessage(testMessage1);
            IMessage reply1 = replyChannel1.Receive(TimeSpan.FromMilliseconds(50));
            Assert.IsNotNull(reply1);
            Assert.That(reply1.Payload, Is.EqualTo("foobar"));
            IMessage reply2 = replyChannel2.Receive(TimeSpan.Zero);
            Assert.IsNull(reply2);
            IMessage testMessage2 =
                MessageBuilder.FromMessage(testMessage1).SetReplyChannelName("replyChannel2").Build();
            endpoint.HandleMessage(testMessage2);
            reply1 = replyChannel1.Receive(TimeSpan.Zero);
            Assert.IsNull(reply1);
            reply2 = replyChannel2.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply2);
            Assert.That(reply2.Payload, Is.EqualTo("foobar"));
        }

        [Test]
        public void NoOutputChannelFallsBackToReturnAddress()
        {
            QueueChannel channel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = CreateEndpoint();
            IMessage message = MessageBuilder.WithPayload("foo").SetReplyChannel(channel).Build();
            endpoint.HandleMessage(message);
            IMessage reply = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("FOO"));
        }

        [Test, ExpectedException(typeof (ChannelResolutionException))]
        public void NoReplyTarget()
        {
            ServiceActivatingHandler endpoint = CreateEndpoint();
            IMessage message = MessageBuilder.WithPayload("foo").Build();
            endpoint.HandleMessage(message);
        }

        [Test]
        public void NoReplyMessage()
        {
            QueueChannel channel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = new ServiceActivatingHandler(new TestNullReplyObject(), "Handle");
            endpoint.OutputChannel = channel;
            IMessage message = MessageBuilder.WithPayload("foo").Build();
            endpoint.HandleMessage(message);
            Assert.IsNull(channel.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (MessagingException)), Ignore("fails")]
        public void NoReplyMessageWithRequiresReply()
        {
            QueueChannel channel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = new ServiceActivatingHandler(new TestNullReplyObject(), "Handle");
            endpoint.RequiresReply = true;
            endpoint.OutputChannel = channel;
            IMessage message = MessageBuilder.WithPayload("foo").Build();
            endpoint.HandleMessage(message);
        }

        [Test]
        public void CorrelationIdNotSetIfMessageIsReturnedUnaltered()
        {
            QueueChannel replyChannel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = new ServiceActivatingHandler(new TestObject3(), "Handle");
            IMessage message = MessageBuilder.WithPayload("test").SetReplyChannel(replyChannel).Build();
            endpoint.HandleMessage(message);
            IMessage reply = replyChannel.Receive(TimeSpan.FromMilliseconds(500));
            Assert.IsNull(reply.Headers.CorrelationId);
        }

        [Test]
        public void CorrelationIdSetByHandlerTakesPrecedence()
        {
            QueueChannel replyChannel = new QueueChannel(1);
            ServiceActivatingHandler endpoint = new ServiceActivatingHandler(new TestObject4(), "Handle");
            IMessage message = MessageBuilder.WithPayload("test").SetReplyChannel(replyChannel).Build();
            endpoint.HandleMessage(message);
            IMessage reply = replyChannel.Receive(TimeSpan.FromMilliseconds(500));
            Object correlationId = reply.Headers.CorrelationId;
            Assert.IsFalse(message.Headers.Id.Equals(correlationId));
            Assert.That(correlationId, Is.EqualTo("ABC-123"));
        }


        private static ServiceActivatingHandler CreateEndpoint()
        {
            return new ServiceActivatingHandler(new TestObject(), "Handle");
        }

        private class TestObject
        {
            public IMessage Handle(IMessage message)
            {
                return new StringMessage(message.Payload.ToString().ToUpper());
            }
        }

        private class TestNullReplyObject
        {
            public IMessage Handle(IMessage message)
            {
                return null;
            }
        }

        private class TestObject2
        {
            public IMessage Handle(IMessage message)
            {
                return new StringMessage("foo" + message.Payload);
            }
        }

        private class TestObject3
        {
            public IMessage Handle(IMessage message)
            {
                return message;
            }
        }

        private class TestObject4
        {
            public IMessage Handle(IMessage message)
            {
                return MessageBuilder.FromMessage(message).SetCorrelationId("ABC-123").Build();
            }
        }
    }
}