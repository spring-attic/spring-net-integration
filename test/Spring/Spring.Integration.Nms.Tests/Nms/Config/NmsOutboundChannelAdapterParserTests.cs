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
using Spring.Context;
using Spring.Integration.Endpoint;
using Spring.Integration.Tests.Util;

namespace Spring.Integration.Nms.Config
{
    [TestFixture]
    public class NmsOutboundChannelAdapterParserTests
    {
        [Test]
        public void AdapterWithConnectionFactoryAndDestination()
        {
            IApplicationContext ctx = NmsTestUtils.GetContext(@"Nms\Config\NmsOutboundWithConnectionFactoryAndDestination.xml");
            EventDrivenConsumer endpoint = (EventDrivenConsumer)ctx.GetObject("adapter");
            object handler = TestUtils.GetFieldValue(endpoint, "_handler");
            Assert.That(handler, Is.Not.Null);
            object template = TestUtils.GetFieldValue(handler, "nmsTemplate");
            Assert.That(template, Is.Not.Null);            
        }
    }
}