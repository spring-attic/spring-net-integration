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
using System.Threading;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Message.Generic;
using Spring.Integration.Selector;
using Spring.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Execution;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class QueueChannelTests
    {
        [Test]
        public void TestSimpleSendAndReceive()
        {
            AtomicBoolean messageReceived = new AtomicBoolean(false);
            CountDownLatch latch = new CountDownLatch(1);
            QueueChannel channel = new QueueChannel();
            new Thread(new ThreadStart(delegate
                                           {
                                               IMessage message = channel.Receive();
                                               if (message != null)
                                               {
                                                   messageReceived.Value = true;
                                                   latch.CountDown();
                                               }
                                           })).Start();

            Assert.That(messageReceived.Value, Is.False);
            channel.Send(new Message<string>("testing"));
            latch.Await(new TimeSpan(0, 0, 0, 0, 25));
            Assert.That(messageReceived.Value, Is.True);
        }

        [Test]
        public void TestImmediateReceive()
        {
            AtomicBoolean messageReceived = new AtomicBoolean(false);
            QueueChannel channel = new QueueChannel();
            CountDownLatch latch1 = new CountDownLatch(1);
            CountDownLatch latch2 = new CountDownLatch(1);
            IExecutor singleThreadExecutor = Executors.NewSingleThreadExecutor();

            singleThreadExecutor.Execute(
                delegate
                    {
                        IMessage message = channel.Receive(TimeSpan.Zero);
                        if (message != null)
                        {
                            messageReceived.Value = true;
                        }
                        latch1.CountDown();
                    });

            latch1.Await();
            singleThreadExecutor.Execute(
                delegate { channel.Send(new Message<string>("testing")); });

            Assert.IsFalse(messageReceived.Value);
            singleThreadExecutor.Execute(
                delegate
                    {
                        IMessage message = channel.Receive(TimeSpan.Zero);
                        if (message != null)
                        {
                            messageReceived.Value = true;
                        }
                        latch2.CountDown();
                    });

            latch2.Await();
            Assert.IsNotNull(messageReceived.Value);
        }

        [Test]
        public void TestBlockingReceiveWithNoTimeout()
        {
            QueueChannel channel = new QueueChannel();
            AtomicBoolean receiveInterrupted = new AtomicBoolean(false);
            CountDownLatch latch = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(delegate
                                                      {
                                                          IMessage message = channel.Receive();
                                                          receiveInterrupted.Value = true;
                                                          Assert.IsTrue(message == null);
                                                          latch.CountDown();
                                                      }));
            t.Start();
            Assert.IsFalse(receiveInterrupted.Value);
            t.Interrupt();
            latch.Await();
            Assert.IsTrue(receiveInterrupted.Value);
        }

        [Test]
        public void TestBlockingReceiveWithTimeout()
        {
            QueueChannel channel = new QueueChannel();
            AtomicBoolean receiveInterrupted = new AtomicBoolean(false);
            CountDownLatch latch = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(delegate
                                                      {
                                                          IMessage message = channel.Receive(new TimeSpan(10000));
                                                          receiveInterrupted.Value = true;
                                                          Assert.IsTrue(message == null);
                                                          latch.CountDown();
                                                      }));
            t.Start();
            //Assert.IsFalse(receiveInterrupted.Value);
            t.Interrupt();
            latch.Await();
            Assert.IsTrue(receiveInterrupted.Value);
        }


        [Test]
        public void TestImmediateSend()
        {
            QueueChannel channel = new QueueChannel(3);
            bool result1 = channel.Send(new Message<string>("test-1"));
            Assert.IsTrue(result1);
            bool result2 = channel.Send(new Message<string>("test-2"), new TimeSpan(100));
            Assert.IsTrue(result2);
            bool result3 = channel.Send(new Message<string>("test-3"), TimeSpan.Zero);
            Assert.IsTrue(result3);
            bool result4 = channel.Send(new Message<string>("test-4"), TimeSpan.Zero);
            Assert.IsFalse(result4);
        }

        [Test]
        public void TestBlockingSendWithNoTimeout()
        {
            QueueChannel channel = new QueueChannel(1);
            bool result1 = channel.Send(new Message<string>("test-1"));
            Assert.IsTrue(result1);
            AtomicBoolean SendInterrupted = new AtomicBoolean(false);
            CountDownLatch latch = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(delegate
                                                      {
                                                          channel.Send(new Message<string>("test-2"));
                                                          SendInterrupted.Value = true;
                                                          latch.CountDown();
                                                      }));
            t.Start();
            //Assert.IsFalse(SendInterrupted.Value);
            t.Interrupt();
            latch.Await();
            Assert.IsTrue(SendInterrupted.Value);
        }

        [Test]
        public void TestBlockingSendWithTimeout()
        {
            QueueChannel channel = new QueueChannel(1);
            bool result1 = channel.Send(new Message<string>("test-1"));
            Assert.IsTrue(result1);
            AtomicBoolean SendInterrupted = new AtomicBoolean(false);
            CountDownLatch latch = new CountDownLatch(1);
            Thread t = new Thread(new ThreadStart(delegate
                                                      {
                                                          channel.Send(new Message<string>("test-2"),
                                                                       TimeSpan.FromMilliseconds(10000));
                                                          SendInterrupted.Value = true;
                                                          latch.CountDown();
                                                      }));
            t.Start();
            //Assert.IsFalse(SendInterrupted.get());
            t.Interrupt();
            latch.Await();
            Assert.IsTrue(SendInterrupted.Value);
        }

        [Test]
        public void TestClear()
        {
            QueueChannel channel = new QueueChannel(2);
            StringMessage message1 = new StringMessage("test1");
            StringMessage message2 = new StringMessage("test2");
            StringMessage message3 = new StringMessage("test3");
            Assert.IsTrue(channel.Send(message1));
            Assert.IsTrue(channel.Send(message2));
            Assert.IsFalse(channel.Send(message3, TimeSpan.Zero));
            IList<IMessage> clearedMessages = channel.Clear();
            Assert.IsNotNull(clearedMessages);
            Assert.That(clearedMessages.Count, Is.EqualTo(2));
            Assert.IsTrue(channel.Send(message3));
        }

        [Test]
        public void TestClearEmptyChannel()
        {
            QueueChannel channel = new QueueChannel();
            IList<IMessage> clearedMessages = channel.Clear();
            Assert.IsNotNull(clearedMessages);
            Assert.That(clearedMessages.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestPurge()
        {
            QueueChannel channel = new QueueChannel(2);
            TimeSpan minute = new TimeSpan(0, 0, 1, 0);
            DateTime now = DateTime.Now;
            DateTime past = now.Subtract(minute);
            DateTime future = now.Add(minute);
            IMessage expiredMessage = MessageBuilder.WithPayload("test1").SetExpirationDate(past).Build();
            IMessage unexpiredMessage = MessageBuilder.WithPayload("test2").SetExpirationDate(future).Build();
            Assert.IsTrue(channel.Send(expiredMessage, TimeSpan.Zero));
            Assert.IsTrue(channel.Send(unexpiredMessage, TimeSpan.Zero));
            Assert.IsFalse(channel.Send(new StringMessage("atCapacity"), TimeSpan.Zero));
            IList<IMessage> purgedMessages = channel.Purge(new UnexpiredMessageSelector());
            Assert.IsNotNull(purgedMessages);
            Assert.That(purgedMessages.Count, Is.EqualTo(1));
            Assert.IsTrue(channel.Send(new StringMessage("roomAvailable"), TimeSpan.Zero));
        }
    }
}