#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
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
using Spring.Context.Support;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Tests.Util;
using Spring.Messaging.Nms.Listener;
using Spring.Objects.Factory;

#endregion

namespace Spring.Integration.Nms.Config
{
    [TestFixture]
    public class NmsInboundGatewayParserTests
    {
        [Test]
        public void GatewayWithConnectionFactoryAndDestination()
        {
            XmlApplicationContext ctx = (XmlApplicationContext) NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithConnectionFactoryAndDestination.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("requestChannel");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("nmsGateway");
            Assert.That(gateway.GetType(), Is.EqualTo(typeof (NmsMessageDrivenEndpoint)));
            ctx.Start();
            ThreadPerTaskExecutor executor = new ThreadPerTaskExecutor();
            executor.Execute(delegate
                                 {
                                     SimpleMessageListenerContainer listenerContainer =
                                         (SimpleMessageListenerContainer)
                                         ctx.GetObject("Spring.Messaging.Nms.Listener.SimpleMessageListenerContainer#0");
                                     ISessionAwareMessageListener messageListener =
                                         (ISessionAwareMessageListener) listenerContainer.MessageListener;
                                     messageListener.OnMessage(new StubTextMessage("message-driven-test"),
                                                               new StubSession("message-driven-test"));
                                 });

            IMessage message = channel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("message-driven-test"));
            ctx.Stop();
        }

        [Test]
        public void GatewayWithConnectionFactoryAndDestinationName()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext)
                NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithConnectionFactoryAndDestinationName.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("requestChannel");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("nmsGateway");
            Assert.That(gateway.GetType(), Is.EqualTo(typeof (NmsMessageDrivenEndpoint)));
            ctx.Start();

            ThreadPerTaskExecutor executor = new ThreadPerTaskExecutor();
            executor.Execute(delegate
                                 {
                                     SimpleMessageListenerContainer listenerContainer =
                                         (SimpleMessageListenerContainer)
                                         ctx.GetObject("Spring.Messaging.Nms.Listener.SimpleMessageListenerContainer#0");
                                     ISessionAwareMessageListener messageListener =
                                         (ISessionAwareMessageListener) listenerContainer.MessageListener;
                                     messageListener.OnMessage(new StubTextMessage("message-driven-test"),
                                                               new StubSession("message-driven-test"));
                                 });

            IMessage message = channel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("message-driven-test"));
            ctx.Stop();
        }


        [Test]
        public void GatewayWithMessageConverter()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext) NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithMessageConverter.xml");
            IPollableChannel channel = (IPollableChannel) ctx.GetObject("requestChannel");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("nmsGateway");
            Assert.That(gateway.GetType(), Is.EqualTo(typeof (NmsMessageDrivenEndpoint)));
            ctx.Start();

            ThreadPerTaskExecutor executor = new ThreadPerTaskExecutor();
            executor.Execute(delegate
                                 {
                                     SimpleMessageListenerContainer listenerContainer =
                                         (SimpleMessageListenerContainer)
                                         ctx.GetObject("Spring.Messaging.Nms.Listener.SimpleMessageListenerContainer#0");
                                     ISessionAwareMessageListener messageListener =
                                         (ISessionAwareMessageListener) listenerContainer.MessageListener;
                                     messageListener.OnMessage(new StubTextMessage("test-message"),
                                                               new StubSession("test-message"));
                                 });

            IMessage message = channel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("converted-test-message"));
            ctx.Stop();
        }

        [Test]
        public void GatewayWithDefaultExtractPayload()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext)
                NmsTestUtils.GetContext(@"Nms\Config\NmsGatewaysWithExtractPayloadAttributes.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("defaultGateway");
            object handler = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listener"), "extractReplyPayload");
            Assert.That(handler,
                        Is.EqualTo(true),
                        "The inbound gateway extract-reply-payload is not configured to the correct value.");
        }

        [Test]
        public void GatewayWithExtractReplyPayloadFalse()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext)
                NmsTestUtils.GetContext(@"Nms\Config\NmsGatewaysWithExtractPayloadAttributes.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("extractReplyPayloadFalse");
            object handler = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listener"), "extractReplyPayload");
            Assert.That(handler,
                        Is.EqualTo(false),
                        "The inbound gateway extract-reply-payload is not configured to the correct value.");
        }

        [Test]
        public void GatewayWithExtractRequestPayloadTrue()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext)
                NmsTestUtils.GetContext(@"Nms\Config\NmsGatewaysWithExtractPayloadAttributes.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("extractRequestPayloadTrue");
            object handler = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listener"),
                                                     "extractRequestPayload");
            Assert.That(handler,
                        Is.EqualTo(true),
                        "The inbound gateway extract-request-payload is not configured to the correct value.");
        }

        [Test]
        public void GatewayWithExtractRequestPayloadFalse()
        {
            XmlApplicationContext ctx =
                (XmlApplicationContext)
                NmsTestUtils.GetContext(@"Nms\Config\NmsGatewaysWithExtractPayloadAttributes.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint) ctx.GetObject("extractRequestPayloadFalse");
            object handler = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listener"),
                                                     "extractRequestPayload");
            Assert.That(handler,
                        Is.EqualTo(false),
                        "The inbound gateway extract-request-payload is not configured to the correct value.");
        }



        [Test]
        [ExpectedException(typeof (ObjectDefinitionStoreException))]
        public void GatewayWithConnectionFactoryOnly()
        {
            try
            {
                XmlApplicationContext ctx =
                    (XmlApplicationContext)
                    NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithConnectionFactoryOnly.xml");
            }
            catch (ObjectDefinitionStoreException e)
            {
                Assert.That(e.Message.Contains("request-destination"), Is.True);
                Assert.That(e.Message.Contains("request-destination-name"), Is.True);
                throw;
            }
        }

        [Test]
        public void GatewayWithEmptyConnectionFactory()
        {
            Assert.Fail("todo");
        }

        [Test]
        public void GatewayWithDefaultConnectionFactory()
        {
            XmlApplicationContext ctx = (XmlApplicationContext) NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithDefaultConnectionFactory.xml");
            IPollableChannel channel = (IPollableChannel)ctx.GetObject("requestChannel");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint)ctx.GetObject("nmsGateway");
            Assert.That(gateway.GetType(), Is.EqualTo(typeof(NmsMessageDrivenEndpoint)));
            ctx.Start();

            ThreadPerTaskExecutor executor = new ThreadPerTaskExecutor();
            executor.Execute(delegate
            {
                SimpleMessageListenerContainer listenerContainer =
                    (SimpleMessageListenerContainer)
                    ctx.GetObject("Spring.Messaging.Nms.Listener.SimpleMessageListenerContainer#0");
                ISessionAwareMessageListener messageListener =
                    (ISessionAwareMessageListener)listenerContainer.MessageListener;
                messageListener.OnMessage(new StubTextMessage("message-driven-test"),
                                          new StubSession("message-driven-test"));
            });

            IMessage message = channel.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Payload, Is.EqualTo("message-driven-test"));
            ctx.Stop();
        }


        /* TODO Implement when we have DefaultMessageListenerContainer
        [Test]
        public void TransactionManagerIsNullByDefault()
        {
            XmlApplicationContext ctx = (XmlApplicationContext) NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayTransactionManagerTests.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint)ctx.GetObject("gatewayWithoutTransactionManager");
            object txManager = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listenerContainer"),
                                                     "transactionManager");
            Assert.That(txManager, Is.Not.Null);
        }*/

        [Test]
        public void GatewayWithConcurrentConsumers()
        {
            XmlApplicationContext ctx = (XmlApplicationContext)NmsTestUtils.GetContext(@"Nms\Config\NmsGatewayWithContainerSettings.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint)ctx.GetObject("gatewayWithConcurrentConsumers");
            gateway.Start();
            object cc = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listenerContainer"),
                                                     "concurrentConsumers");
            Assert.That(cc, Is.EqualTo(3));
            gateway.Stop();
        }

        /* TODO Implement when we have DefaultMessageListenerContainer
        [Test]
        public void GatewayWithMaxMessagesPerTask()
        {
            
        }

        [Test]
        public void GatewayWithIdleTaskExecutionLimit()
        {
            
        }*/

        [Test]
        public void GatewayWithContainerReference()
        {
            XmlApplicationContext ctx = (XmlApplicationContext)NmsTestUtils.GetContext(@"Nms\Config\InboundGatewayWithContainerReference.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint)ctx.GetObject("gatewayWithContainerReference");
            gateway.Start();
            object cc = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listenerContainer"),
                                                     "concurrentConsumers");
            Assert.That(cc, Is.EqualTo(1)); //default value of concurrent consumers.
            gateway.Stop();
        }
    }
}