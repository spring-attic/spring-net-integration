#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
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
using Spring.Objects.Factory;

#endregion

namespace Spring.Integration.Nms.Config
{
    [TestFixture]
    public class NmsInboundChannelAdapterParserTests
    {
        private TimeSpan timeoutOnReceive = TimeSpan.FromMilliseconds(3000);

        [Test]
        public void AdapterWithJmsTemplate()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithNmsTemplate.xml");
            IPollableChannel output = (IPollableChannel) ctx.GetObject("output");
            IMessage message = output.Receive(timeoutOnReceive);
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("polling-test"));
        }

        [Test]
        public void AdapterWithConnectionFactoryAndDestination()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithConnectionFactoryAndDestinationName.xml");
            IPollableChannel output = (IPollableChannel)ctx.GetObject("output");
            IMessage message = output.Receive(timeoutOnReceive);
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("polling-test"));
        }

        [Test]
        [ExpectedException(typeof(ObjectDefinitionStoreException))]
        public void AdapterWithConnectionFactoryOnly()
        {
            try
            {
                IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithConnectionFactoryOnly.xml");
            } catch (Exception ex)
            {
                Assert.That(ex.InnerException.GetType(), Is.EqualTo(typeof(ObjectCreationException)));
                throw;
            }
        }

        [Test]
        [ExpectedException(typeof(ObjectCreationException))]
        public void AdapterWithDestinationOnly()
        {
            try
            {
                IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithDestinationOnly.xml");
            }
            catch (Exception ex)
            {
                Assert.That(ex.InnerException.GetType(), Is.EqualTo(typeof(NoSuchObjectDefinitionException)));
                throw;
            }
        }

        [Test]
        public void AdpaterWithDestinationAndDefaultConnectionFactory()
        {
            XmlApplicationContext ctx = (XmlApplicationContext) NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithDestinationAndDefaultConnectionFactory.xml");
            IPollableChannel output = (IPollableChannel)ctx.GetObject("output");
            IMessage message = output.Receive(timeoutOnReceive);
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("polling-test"));
            ctx.Stop();
        }


        [Test]
        [ExpectedException(typeof(ObjectCreationException))]
        public void AdapterWithDestinationNameOnly()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithDestinationNameOnly.xml");           
        }

        [Test]
        public void AdapterWithDestinationNameAndDefaultConnectionFactory()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithDestinationNameAndDefaultConnectionFactory.xml");
            IPollableChannel output = (IPollableChannel)ctx.GetObject("output");
            IMessage message = output.Receive(timeoutOnReceive);
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("polling-test"));
        }

        [Test]
        public void AdapterWithHeaderMapper()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsInboundWithHeaderMapper.xml");
            IPollableChannel output = (IPollableChannel)ctx.GetObject("output");
            IMessage message = output.Receive(timeoutOnReceive);
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("polling-test"));
            Assert.That(message.Headers["testProperty"], Is.EqualTo("foo"));
            Assert.That(message.Headers["testAttribute"], Is.EqualTo(123));
        }

    }
}