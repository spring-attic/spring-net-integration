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


using NUnit.Framework;
using Spring.Context.Support;
using Spring.Integration.Endpoint;
using Spring.Integration.Tests.Util;

namespace Spring.Integration.Nms.Config
{
    [TestFixture]
    public class NmsOutboundGatewayParserTests
    {
        /*            XmlApplicationContext ctx = (XmlApplicationContext)NmsTestUtils.GetContext(@"Nms\Config\InboundGatewayWithContainerReference.xml");
            NmsMessageDrivenEndpoint gateway = (NmsMessageDrivenEndpoint)ctx.GetObject("gatewayWithContainerReference");
            gateway.Start();
            object cc = TestUtils.GetFieldValue(TestUtils.GetFieldValue(gateway, "listenerContainer"),
                                                     "concurrentConsumers");
            Assert.That(cc, Is.EqualTo(1)); //default value of concurrent consumers.
            gateway.Stop();
         */
        [Test]
        public void Default()
        {
            XmlApplicationContext ctx = (XmlApplicationContext)NmsTestUtils.GetContext(@"Nms\Config\NmsOutboundGatewayWithConverter.xml");
            PollingConsumer endpoint = (PollingConsumer) ctx.GetObject("nmsGateway");
            // NmsOutboundGateway._handler.
            object messageConverter = TestUtils.GetFieldValue(TestUtils.GetFieldValue(endpoint, "_handler"),
                                                              "messageConverter");

            Assert.That(messageConverter, Is.InstanceOf(typeof(StubMessageConverter)));
        }

        [Test]
        public void GatewayWithOrder()
        {
            XmlApplicationContext ctx = (XmlApplicationContext)NmsTestUtils.GetContext(@"Nms\Config\NmsOutboundGatewayWithOrder.xml");
            EventDrivenConsumer endpoint = (EventDrivenConsumer)ctx.GetObject("nmsGateway");
            // NmsOutboundGateway._handler.
            object order = TestUtils.GetFieldValue(TestUtils.GetFieldValue(endpoint, "_handler"),
                                                              "order");

            Assert.That(order, Is.EqualTo(99));
        }
    }
}           