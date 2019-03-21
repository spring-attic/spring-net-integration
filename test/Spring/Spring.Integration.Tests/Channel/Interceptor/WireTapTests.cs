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
using Spring.Integration.Selector;

#endregion

namespace Spring.Integration.Tests.Channel.Interceptor
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class WireTapTests
    {
        [Test]
        public void WireTapWithNoSelector()
        {
            QueueChannel mainChannel = new QueueChannel();
            QueueChannel secondaryChannel = new QueueChannel();
            mainChannel.AddInterceptor(new WireTap(secondaryChannel));
            mainChannel.Send(new StringMessage("testing"));
            IMessage original = mainChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(original);
            IMessage intercepted = secondaryChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(intercepted);
            Assert.That(original, Is.EqualTo(intercepted));
        }

        [Test]
        public void WireTapWithRejectingSelector()
        {
            QueueChannel mainChannel = new QueueChannel();
            QueueChannel secondaryChannel = new QueueChannel();
            mainChannel.AddInterceptor(new WireTap(secondaryChannel, new TestSelector(false)));
            mainChannel.Send(new StringMessage("testing"));
            IMessage original = mainChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(original);
            IMessage intercepted = secondaryChannel.Receive(TimeSpan.Zero);
            Assert.IsNull(intercepted);
        }

        [Test]
        public void WireTapWithAcceptingSelector()
        {
            QueueChannel mainChannel = new QueueChannel();
            QueueChannel secondaryChannel = new QueueChannel();
            mainChannel.AddInterceptor(new WireTap(secondaryChannel, new TestSelector(true)));
            mainChannel.Send(new StringMessage("testing"));
            IMessage original = mainChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(original);
            IMessage intercepted = secondaryChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(intercepted);
            Assert.That(original, Is.EqualTo(intercepted));
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void WireTapTargetMustNotBeNull()
        {
            new WireTap(null);
        }

        [Test]
        public void SimpleTargetWireTap()
        {
            QueueChannel mainChannel = new QueueChannel();
            QueueChannel secondaryChannel = new QueueChannel();
            mainChannel.AddInterceptor(new WireTap(secondaryChannel));
            Assert.IsNull(secondaryChannel.Receive(TimeSpan.Zero));
            mainChannel.Send(new StringMessage("testing"));
            IMessage original = mainChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(original);
            IMessage intercepted = secondaryChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(intercepted);
            Assert.That(original, Is.EqualTo(intercepted));
        }

        [Test]
        public void InterceptedMessageContainsHeaderValue()
        {
            QueueChannel mainChannel = new QueueChannel();
            QueueChannel secondaryChannel = new QueueChannel();
            mainChannel.AddInterceptor(new WireTap(secondaryChannel));
            string headerName = "testAttribute";
            IMessage message = MessageBuilder.WithPayload("testing").SetHeader(headerName, 123).Build();
            mainChannel.Send(message);
            IMessage original = mainChannel.Receive(TimeSpan.Zero);
            IMessage intercepted = secondaryChannel.Receive(TimeSpan.Zero);
            object originalAttribute = original.Headers.Get(headerName);
            object interceptedAttribute = intercepted.Headers.Get(headerName);
            Assert.IsNotNull(originalAttribute);
            Assert.IsNotNull(interceptedAttribute);
            Assert.That(original, Is.EqualTo(intercepted));
        }


        private class TestSelector : IMessageSelector
        {
            private readonly bool _shouldAccept;

            public TestSelector(bool shouldAccept)
            {
                _shouldAccept = shouldAccept;
            }

            public bool Accept(IMessage message)
            {
                return _shouldAccept;
            }
        }
    }
}