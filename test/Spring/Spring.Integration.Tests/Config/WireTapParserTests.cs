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
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class WireTapParserTests
    {
        [Test]
        public void simpleWireTap()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\WireTapParserTests.xml");
            IMessageChannel mainChannel = (IMessageChannel) ctx.GetObject("noSelectors");
            IPollableChannel wireTapChannel = (IPollableChannel) ctx.GetObject("wireTapChannel");
            Assert.IsNull(wireTapChannel.Receive(TimeSpan.Zero));
            IMessage original = new StringMessage("test");
            mainChannel.Send(original);
            IMessage intercepted = wireTapChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(intercepted);
            Assert.That(intercepted, Is.EqualTo(original));
        }

        [Test]
        public void wireTapWithAcceptingSelector()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\WireTapParserTests.xml");
            IMessageChannel mainChannel = (IMessageChannel) ctx.GetObject("accepting");
            IPollableChannel wireTapChannel = (IPollableChannel) ctx.GetObject("wireTapChannel");
            Assert.IsNull(wireTapChannel.Receive(TimeSpan.Zero));
            IMessage original = new StringMessage("test");
            mainChannel.Send(original);
            IMessage intercepted = wireTapChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(intercepted);
            Assert.That(intercepted, Is.EqualTo(original));
        }

        [Test]
        public void wireTapWithRejectingSelector()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\WireTapParserTests.xml");
            IMessageChannel mainChannel = (IMessageChannel) ctx.GetObject("rejecting");
            IPollableChannel wireTapChannel = (IPollableChannel) ctx.GetObject("wireTapChannel");
            Assert.IsNull(wireTapChannel.Receive(TimeSpan.Zero));
            IMessage original = new StringMessage("test");
            mainChannel.Send(original);
            IMessage intercepted = wireTapChannel.Receive(TimeSpan.Zero);
            Assert.IsNull(intercepted);
        }

        [Test]
        public void wireTapTimeouts()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\WireTapParserTests.xml");
            IDictionary wireTaps = ctx.GetObjectsOfType(typeof (WireTap));
            int defaultTimeoutCount = 0;
            int expectedTimeoutCount = 0;
            int otherTimeoutCount = 0;
            foreach (WireTap wireTap in wireTaps.Values)
            {
                FieldInfo fi = typeof (WireTap).GetField("_timeout", BindingFlags.Instance | BindingFlags.NonPublic);

                TimeSpan timeout = (TimeSpan) fi.GetValue(wireTap);
                if (timeout == TimeSpan.Zero)
                {
                    defaultTimeoutCount++;
                }
                else if (timeout.TotalMilliseconds == 1234)
                {
                    expectedTimeoutCount++;
                }
                else
                {
                    otherTimeoutCount++;
                }
            }
            Assert.That(defaultTimeoutCount, Is.EqualTo(3));
            Assert.That(expectedTimeoutCount, Is.EqualTo(1));
            Assert.That(otherTimeoutCount, Is.EqualTo(0));
        }
    }
}