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
using Spring.Context;
using Spring.Integration.Core;
using Spring.Integration.Message.Generic;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class EndpointParserTests
    {
        [Test]
        public void TestSimpleEndpoint()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\simpleEndpointTests.xml");

            //context.start();
            IMessageChannel channel = (IMessageChannel) ctx.GetObject("testChannel");
            TestHandler handler = (TestHandler) ctx.GetObject("testHandler");
            Assert.IsNull(handler.MessageString);
            channel.Send(new Message<String>("test"));
            handler.Latch.Await(TimeSpan.FromMilliseconds(500));
            Assert.That(handler.MessageString, Is.EqualTo("test"));
        }
    }
}