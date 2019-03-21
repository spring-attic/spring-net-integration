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
using Spring.Integration.Channel;
using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Threading.AtomicTypes;

#endregion

namespace Spring.Integration.Tests.Channel.Interceptor
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class ChannelInterceptorTests
    {
        [Test]
        public void TestPreSendInterceptorReturnsMessage()
        {
            QueueChannel channel = new QueueChannel();

            channel.AddInterceptor(new PreSendReturnsMessageInterceptor());
            channel.Send(new StringMessage("test"));
            IMessage result = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(result);
            Assert.That("test", Is.EqualTo(result.Payload));
            Assert.That(result.Headers.Get(typeof (PreSendReturnsMessageInterceptor).Name), Is.EqualTo(1));
        }

        [Test]
        public void TestPreSendInterceptorReturnsNull()
        {
            QueueChannel channel = new QueueChannel();
            PreSendReturnsNullInterceptor interceptor = new PreSendReturnsNullInterceptor();
            channel.AddInterceptor(interceptor);
            IMessage message = new StringMessage("test");
            channel.Send(message);
            Assert.That(interceptor.Count, Is.EqualTo(1));
            IMessage result = channel.Receive(TimeSpan.Zero);
            Assert.IsNull(result);
        }

        private class TestPostSendInterceptorWithSentMessageInterceptor : ChannelInterceptorAdapter
        {
            private readonly AtomicBoolean _invoked;
            private readonly IMessageChannel _channel;

            public TestPostSendInterceptorWithSentMessageInterceptor(AtomicBoolean invoked, IMessageChannel channel)
            {
                _invoked = invoked;
                _channel = channel;
            }

            public override void PostSend(IMessage message, IMessageChannel channel, bool sent)
            {
                Assert.IsNotNull(message);
                Assert.IsNotNull(channel);
                Assert.That(channel, Is.SameAs(_channel));
                Assert.IsTrue(sent);
                _invoked.Value = true;
            }
        }

        [Test]
        public void TestPostSendInterceptorWithSentMessage()
        {
            QueueChannel channel = new QueueChannel();
            AtomicBoolean invoked = new AtomicBoolean(false);

            channel.AddInterceptor(new TestPostSendInterceptorWithSentMessageInterceptor(invoked, channel));

            channel.Send(new StringMessage("test"));
            Assert.IsTrue(invoked.Value);
        }

        private class TestPostSendInterceptorWithUnsentMessageInterceptor : ChannelInterceptorAdapter
        {
            private readonly AtomicInteger _invokedCounter;
            private readonly AtomicInteger _sentCounter;
            private readonly IMessageChannel _channel;

            public TestPostSendInterceptorWithUnsentMessageInterceptor(AtomicInteger invokedCounter,
                                                                       AtomicInteger sentCounter,
                                                                       IMessageChannel channel)
            {
                _invokedCounter = invokedCounter;
                _sentCounter = sentCounter;
                _channel = channel;
            }

            public override void PostSend(IMessage message, IMessageChannel channel, bool sent)
            {
                Assert.IsNotNull(message);
                Assert.IsNotNull(channel);
                Assert.That(channel, Is.SameAs(_channel));

                if (sent)
                {
                    _sentCounter.IncrementValueAndReturn();
                }
                _invokedCounter.IncrementValueAndReturn();
            }
        }

        [Test]
        public void TestPostSendInterceptorWithUnsentMessage()
        {
            AtomicInteger invokedCounter = new AtomicInteger(0);
            AtomicInteger sentCounter = new AtomicInteger(0);
            QueueChannel singleItemChannel = new QueueChannel(1);
            singleItemChannel.AddInterceptor(new TestPostSendInterceptorWithUnsentMessageInterceptor(invokedCounter,
                                                                                                     sentCounter,
                                                                                                     singleItemChannel));
            Assert.That(invokedCounter.Value, Is.EqualTo(0));
            Assert.That(sentCounter.Value, Is.EqualTo(0));
            singleItemChannel.Send(new StringMessage("test1"));
            Assert.That(invokedCounter.Value, Is.EqualTo(1));
            Assert.That(sentCounter.Value, Is.EqualTo(1));
            singleItemChannel.Send(new StringMessage("test2"), TimeSpan.Zero);
            Assert.That(invokedCounter.Value, Is.EqualTo(2));
            Assert.That(sentCounter.Value, Is.EqualTo(1));
        }

        [Test]
        public void TestPreReceiveInterceptorReturnsTrue()
        {
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(new PreReceiveReturnsTrueInterceptor());
            IMessage message = new StringMessage("test");
            channel.Send(message);
            IMessage result = channel.Receive(TimeSpan.Zero);
            Assert.That(PreReceiveReturnsTrueInterceptor.counter.Value, Is.EqualTo(1));
            Assert.IsNotNull(result);
        }

        [Test]
        public void TestPreReceiveInterceptorReturnsFalse()
        {
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(new PreReceiveReturnsFalseInterceptor());
            IMessage message = new StringMessage("test");
            channel.Send(message);
            IMessage result = channel.Receive(TimeSpan.Zero);
            Assert.That(PreReceiveReturnsFalseInterceptor.counter.Value, Is.EqualTo(1));
            Assert.IsNull(result);
        }


        private class TestPostReceiveInterceptorInterceptor : ChannelInterceptorAdapter
        {
            private readonly AtomicInteger _invokedCounter;
            private readonly AtomicInteger _messageCounter;
            private readonly IMessageChannel _channel;

            public TestPostReceiveInterceptorInterceptor(AtomicInteger invokedCounter, AtomicInteger messageCounter,
                                                         IMessageChannel channel)
            {
                _invokedCounter = invokedCounter;
                _messageCounter = messageCounter;
                _channel = channel;
            }

            public override IMessage PostReceive(IMessage message, IMessageChannel channel)
            {
                Assert.IsNotNull(channel);
                Assert.That(channel, Is.SameAs(_channel));

                if (message != null)
                {
                    _messageCounter.IncrementValueAndReturn();
                }
                _invokedCounter.IncrementValueAndReturn();
                return message;
            }
        }

        [Test]
        public void TestPostReceiveInterceptor()
        {
            AtomicInteger invokedCount = new AtomicInteger();
            AtomicInteger messageCount = new AtomicInteger();
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(new TestPostReceiveInterceptorInterceptor(invokedCount, messageCount, channel));
            channel.Receive(TimeSpan.Zero);
            Assert.That(invokedCount.Value, Is.EqualTo(1));
            Assert.That(messageCount.Value, Is.EqualTo(0));
            channel.Send(new StringMessage("test"));
            IMessage result = channel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(result);
            Assert.That(invokedCount.Value, Is.EqualTo(2));
            Assert.That(messageCount.Value, Is.EqualTo(1));
        }


        private class PreSendReturnsMessageInterceptor : ChannelInterceptorAdapter
        {
            private static readonly AtomicInteger counter = new AtomicInteger();

            public override IMessage PreSend(IMessage message, IMessageChannel channel)
            {
                Assert.IsNotNull(message);
                return
                    MessageBuilder.FromMessage(message).SetHeader(GetType().Name, counter.IncrementValueAndReturn()).
                        Build();
            }
        }


        private class PreSendReturnsNullInterceptor : ChannelInterceptorAdapter
        {
            private static readonly AtomicInteger counter = new AtomicInteger();

            public int Count
            {
                get { return counter.Value; }
            }

            public override IMessage PreSend(IMessage message, IMessageChannel channel)
            {
                Assert.IsNotNull(message);
                counter.IncrementValueAndReturn();
                return null;
            }
        }


        private class PreReceiveReturnsTrueInterceptor : ChannelInterceptorAdapter
        {
            public static readonly AtomicInteger counter = new AtomicInteger();

            public override bool PreReceive(IMessageChannel channel)
            {
                counter.IncrementValueAndReturn();
                return true;
            }
        }


        private class PreReceiveReturnsFalseInterceptor : ChannelInterceptorAdapter
        {
            public static readonly AtomicInteger counter = new AtomicInteger();

            public override bool PreReceive(IMessageChannel channel)
            {
                counter.IncrementValueAndReturn();
                return false;
            }
        }
    }
}