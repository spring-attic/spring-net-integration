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

using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Selector;
using Spring.Threading.AtomicTypes;

#endregion

namespace Spring.Integration.Tests.Channel.Interceptor
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class MessageSelectingInterceptorTests
    {
        [Test]
        public void testSingleSelectorAccepts()
        {
            AtomicInteger counter = new AtomicInteger();
            IMessageSelector selector = new TestMessageSelector(true, counter);
            MessageSelectingInterceptor interceptor = new MessageSelectingInterceptor(selector);
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(interceptor);
            Assert.That(channel.Send(new StringMessage("test1")), Is.True);
        }

        [Test, ExpectedException(typeof (MessageDeliveryException))]
        public void testSingleSelectorRejects()
        {
            AtomicInteger counter = new AtomicInteger();
            IMessageSelector selector = new TestMessageSelector(false, counter);
            MessageSelectingInterceptor interceptor = new MessageSelectingInterceptor(selector);
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(interceptor);
            channel.Send(new StringMessage("test1"));
        }

        [Test]
        public void testMultipleSelectorsAccept()
        {
            AtomicInteger counter = new AtomicInteger();
            IMessageSelector selector1 = new TestMessageSelector(true, counter);
            IMessageSelector selector2 = new TestMessageSelector(true, counter);
            MessageSelectingInterceptor interceptor = new MessageSelectingInterceptor(selector1, selector2);
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(interceptor);
            Assert.That(channel.Send(new StringMessage("test1")), Is.True);
            Assert.That(counter.Value, Is.EqualTo(2));
        }

        [Test]
        public void testMultipleSelectorsReject()
        {
            bool exceptionThrown = false;
            AtomicInteger counter = new AtomicInteger();
            IMessageSelector selector1 = new TestMessageSelector(true, counter);
            IMessageSelector selector2 = new TestMessageSelector(false, counter);
            IMessageSelector selector3 = new TestMessageSelector(false, counter);
            IMessageSelector selector4 = new TestMessageSelector(true, counter);
            MessageSelectingInterceptor interceptor = new MessageSelectingInterceptor(selector1, selector2, selector3,
                                                                                      selector4);
            QueueChannel channel = new QueueChannel();
            channel.AddInterceptor(interceptor);
            try
            {
                channel.Send(new StringMessage("test1"));
            }
            catch (MessageDeliveryException)
            {
                exceptionThrown = true;
            }
            Assert.That(exceptionThrown, Is.True);
            Assert.That(counter.Value, Is.EqualTo(2));
        }


        private class TestMessageSelector : IMessageSelector
        {
            private readonly bool _shouldAccept;

            private readonly AtomicInteger _counter;

            public TestMessageSelector(bool shouldAccept, AtomicInteger counter)
            {
                _shouldAccept = shouldAccept;
                _counter = counter;
            }

            public bool Accept(IMessage message)
            {
                _counter.IncrementValueAndReturn();
                return _shouldAccept;
            }
        }
    }
}