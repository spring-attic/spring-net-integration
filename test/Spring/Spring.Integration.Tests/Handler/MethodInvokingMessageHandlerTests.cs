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
using Spring.Integration.Endpoint;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Message.Generic;
using Spring.Integration.Scheduling;
using Spring.Integration.Tests.Util;
using Spring.Threading.Collections.Generic;

#endregion

namespace Spring.Integration.Tests.Handler
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class MethodInvokingMessageHandlerTests
    {
        [Test]
        public void ValidMethod()
        {
            MethodInvokingMessageHandler handler = new MethodInvokingMessageHandler(new TestSink(), "ValidMethod");
            handler.HandleMessage(new Message<string>("test"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void InvalidMethodWithNoArgs()
        {
            new MethodInvokingMessageHandler(new TestSink(), "InvalidMethodWithNoArgs");
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void MethodWithReturnValue()
        {
            IMessage message = new StringMessage("test");
            try
            {
                MethodInvokingMessageHandler handler = new MethodInvokingMessageHandler(new TestSink(),
                                                                                        "methodWithReturnValue");
                handler.HandleMessage(message);
            }
            catch (MessagingException e)
            {
                Assert.That(message, Is.EqualTo(e.FailedMessage));
                throw e;
            }
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void NoMatchingMethodName()
        {
            new MethodInvokingMessageHandler(new TestSink(), "noSuchMethod");
        }

        [Test, Ignore("SynchronousQueue not yet implemented")]
        public void Subscription()
        {
            TestUtils.TestApplicationContext context = TestUtils.CreateTestApplicationContext();
            SynchronousQueue<string> queue = new SynchronousQueue<string>();
            TestBean testBean = new TestBean(queue);
            QueueChannel channel = new QueueChannel();
            context.RegisterChannel("channel", channel);
            Message<string> message = new Message<string>("testing");
            channel.Send(message);
            string polledString;
            Assert.IsFalse(queue.Poll(out polledString));
            MethodInvokingMessageHandler handler = new MethodInvokingMessageHandler(testBean, "foo");
            PollingConsumer endpoint = new PollingConsumer(channel, handler);
            endpoint.Trigger = new IntervalTrigger(TimeSpan.FromMilliseconds(10));
            context.RegisterEndpoint("testEndpoint", endpoint);
            context.Refresh();
            string result;
            Assert.IsTrue(queue.Poll(TimeSpan.FromMilliseconds(1000), out result));
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("testing"));
            context.Stop();
        }


        private class TestBean
        {
            private IBlockingQueue<string> _queue;

            public TestBean(IBlockingQueue<string> queue)
            {
                _queue = queue;
            }

            public void Foo(string s)
            {
                try
                {
                    _queue.Put(s);
                }
                catch (ThreadInterruptedException e)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }


        private class TestSink
        {
            private string result;


            public void ValidMethod(string s)
            {
            }

            public void InvalidMethodWithNoArgs()
            {
            }

            public string MethodWithReturnValue(string s)
            {
                return "value";
            }

            public void Store(string s)
            {
                result = s;
            }

            public string Get()
            {
                return result;
            }
        }
    }
}