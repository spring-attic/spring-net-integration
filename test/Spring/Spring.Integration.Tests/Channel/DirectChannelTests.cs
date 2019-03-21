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
using System.Threading;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Threading;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class DirectChannelTests
    {
        [Test]
        public void testSend()
        {
            DirectChannel channel = new DirectChannel();
            ThreadNameExtractingTestTarget target = new ThreadNameExtractingTestTarget();
            channel.Subscribe(target);
            StringMessage message = new StringMessage("test");
            Assert.IsTrue(channel.Send(message));
            Assert.That(target.ThreadName, Is.EqualTo(Thread.CurrentThread.Name));
        }

        [Test]
        public void testSendInSeparateThread()
        {
            CountDownLatch latch = new CountDownLatch(1);
            DirectChannel channel = new DirectChannel();
            ThreadNameExtractingTestTarget target = new ThreadNameExtractingTestTarget(latch);
            channel.Subscribe(target);
            StringMessage message = new StringMessage("test");
            Thread t = new Thread(new ThreadStart(delegate { channel.Send(message); }));
            t.Name = "test-thread";
            t.Start();
            latch.Await(new TimeSpan(0, 0, 0, 0, 1000));
            Assert.That(target.ThreadName, Is.EqualTo("test-thread"));
        }


        private class ThreadNameExtractingTestTarget : IMessageHandler
        {
            private string _threadName;

            private readonly CountDownLatch _latch;


            public ThreadNameExtractingTestTarget()
                : this(null)
            {
            }

            public ThreadNameExtractingTestTarget(CountDownLatch latch)
            {
                _latch = latch;
            }

            public void HandleMessage(IMessage message)
            {
                _threadName = Thread.CurrentThread.Name;
                if (_latch != null)
                {
                    _latch.CountDown();
                }
            }

            public string ThreadName
            {
                get { return _threadName; }
            }
        }
    }
}