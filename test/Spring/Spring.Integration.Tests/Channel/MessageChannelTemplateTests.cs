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
using Spring.Integration.Endpoint;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Scheduling;
using Spring.Integration.Tests.Util;
using Spring.Threading;

#endregion

namespace Spring.Integration.Tests.Channel
{
    [TestFixture]
    public class MessageChannelTemplateTests
    {
        private readonly TestUtils.TestApplicationContext context = TestUtils.CreateTestApplicationContext();

        private QueueChannel requestChannel;

        private class TestReplyProducingMessageHandler : AbstractReplyProducingMessageHandler
        {
            protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder)
            {
                replyHolder.Set(message.Payload.ToString().ToUpper());
            }
        } ;

        [TestFixtureSetUp]
        public void SetUp()
        {
            requestChannel = new QueueChannel();
            context.RegisterChannel("requestChannel", requestChannel);
            TestReplyProducingMessageHandler handler = new TestReplyProducingMessageHandler();

            PollingConsumer endpoint = new PollingConsumer(requestChannel, handler);
            endpoint.Trigger = new IntervalTrigger(new TimeSpan(0, 0, 0, 0, 10));
            context.RegisterEndpoint("testEndpoint", endpoint);
            context.Refresh();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            try
            {
                context.Stop();
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            {
// ReSharper restore EmptyGeneralCatchClause
                // ignore
            }
        }

        [Test]
        public void Send()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            QueueChannel channel = new QueueChannel();
            template.Send(new StringMessage("test"), channel);
            IMessage reply = channel.Receive();
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void SendWithDefaultChannelProvidedBySetter()
        {
            QueueChannel channel = new QueueChannel();
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.DefaultChannel = channel;
            template.Send(new StringMessage("test"));
            IMessage reply = channel.Receive();
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void SendWithDefaultChannelProvidedByConstructor()
        {
            QueueChannel channel = new QueueChannel();
            MessageChannelTemplate template = new MessageChannelTemplate(channel);
            template.Send(new StringMessage("test"));
            IMessage reply = channel.Receive();
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void SendWithExplicitChannelTakesPrecedenceOverDefault()
        {
            QueueChannel explicitChannel = new QueueChannel();
            QueueChannel defaultChannel = new QueueChannel();
            MessageChannelTemplate template = new MessageChannelTemplate(defaultChannel);
            template.Send(new StringMessage("test"), explicitChannel);
            IMessage reply = explicitChannel.Receive();
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
            Assert.IsNull(defaultChannel.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void SendWithoutChannelArgFailsIfNoDefaultAvailable()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.Send(new StringMessage("test"));
        }

        [Test]
        public void Receive()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test"));
            MessageChannelTemplate template = new MessageChannelTemplate();
            IMessage reply = template.Receive(channel);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void ReceiveWithDefaultChannelProvidedBySetter()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test"));
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.DefaultChannel = channel;
            IMessage reply = template.Receive();
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void ReceiveWithDefaultChannelProvidedByConstructor()
        {
            QueueChannel channel = new QueueChannel();
            channel.Send(new StringMessage("test"));
            MessageChannelTemplate template = new MessageChannelTemplate(channel);
            IMessage reply = template.Receive();
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void ReceiveWithExplicitChannelTakesPrecedenceOverDefault()
        {
            QueueChannel explicitChannel = new QueueChannel();
            QueueChannel defaultChannel = new QueueChannel();
            explicitChannel.Send(new StringMessage("test"));
            MessageChannelTemplate template = new MessageChannelTemplate(defaultChannel);
            template.ReceiveTimeout = TimeSpan.Zero;
            IMessage reply = template.Receive(explicitChannel);
            Assert.That(reply.Payload, Is.EqualTo("test"));
            Assert.IsNull(defaultChannel.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void ReceiveWithoutChannelArgFailsIfNoDefaultAvailable()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.Receive();
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void ReceiveWithNonPollableDefaultFails()
        {
            DirectChannel channel = new DirectChannel();
            MessageChannelTemplate template = new MessageChannelTemplate(channel);
            template.Receive();
        }

        [Test]
        public void SendAndReceive()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            IMessage reply = template.SendAndReceive(new StringMessage("test"), requestChannel);
            Assert.That(reply.Payload, Is.EqualTo("TEST"));
        }

        [Test]
        public void SendAndReceiveWithDefaultChannel()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.DefaultChannel = requestChannel;
            IMessage reply = template.SendAndReceive(new StringMessage("test"));
            Assert.That(reply.Payload, Is.EqualTo("TEST"));
        }

        [Test]
        public void SendAndReceiveWithExplicitChannelTakesPrecedenceOverDefault()
        {
            QueueChannel defaultChannel = new QueueChannel();
            MessageChannelTemplate template = new MessageChannelTemplate(defaultChannel);
            IMessage message = new StringMessage("test");
            IMessage reply = template.SendAndReceive(message, requestChannel);
            Assert.That(reply.Payload, Is.EqualTo("TEST"));
            Assert.IsNull(defaultChannel.Receive(TimeSpan.Zero));
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void SendAndReceiveWithoutChannelArgFailsIfNoDefaultAvailable()
        {
            MessageChannelTemplate template = new MessageChannelTemplate();
            template.SendAndReceive(new StringMessage("test"));
        }


        public class SendWithReturnAddressChannel : AbstractMessageChannel
        {
            private readonly IList<string> _replies;
            private readonly CountDownLatch _latch;

            public SendWithReturnAddressChannel(IList<string> replies, CountDownLatch latch)
            {
                _replies = replies;
                _latch = latch;
            }

            protected override bool DoSend(IMessage message, TimeSpan timeout)
            {
                _replies.Add((string) message.Payload);
                _latch.CountDown();
                return true;
            }
        } ;


        [Test]
        public void sendWithReturnAddress()
        {
            IList<string> replies = new List<string>(3);
            CountDownLatch latch = new CountDownLatch(3);
            IMessageChannel replyChannel = new SendWithReturnAddressChannel(replies, latch);
            MessageChannelTemplate template = new MessageChannelTemplate();
            IMessage message1 = MessageBuilder.WithPayload("test1").SetReplyChannel(replyChannel).Build();
            IMessage message2 = MessageBuilder.WithPayload("test2").SetReplyChannel(replyChannel).Build();
            IMessage message3 = MessageBuilder.WithPayload("test3").SetReplyChannel(replyChannel).Build();
            template.Send(message1, requestChannel);
            template.Send(message2, requestChannel);
            template.Send(message3, requestChannel);
            latch.Await(TimeSpan.FromMilliseconds(2000));
            Assert.That(latch.Count, Is.EqualTo(0));
            Assert.IsTrue(replies.Contains("TEST1"));
            Assert.IsTrue(replies.Contains("TEST2"));
            Assert.IsTrue(replies.Contains("TEST3"));
        }
    }
}