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
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config.Xml
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    //@ContextConfiguration
    public class BridgeParserTests
    {
        //}extends AbstractJUnit4SpringContextTests {

        //@Autowired
        //@Qualifier("pollableChannel")
        //private PollableChannel pollableChannel;

        //@Autowired
        //@Qualifier("subscribableChannel")
        //private MessageChannel subscribableChannel;

        //@Autowired
        //@Qualifier("output1")
        //private PollableChannel output1;

        //@Autowired
        //@Qualifier("output2")
        //private PollableChannel output2;


        [Test]
        public void PollableChannel()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\BridgeParserTests-context.xml");
            IMessage message = new StringMessage("test1");
            IPollableChannel pollableChannel = (IPollableChannel) context.GetObject("pollableChannel");
            pollableChannel.Send(message);
            IPollableChannel output1 = (IPollableChannel) context.GetObject("output1");
            IMessage reply = output1.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.That(reply, Is.EqualTo(message));
        }

        [Test]
        public void SubscribableChannel()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\BridgeParserTests-context.xml");
            IMessage message = new StringMessage("test2");
            IMessageChannel subscribableChannel = (IMessageChannel) context.GetObject("subscribableChannel");
            subscribableChannel.Send(message);
            IPollableChannel output2 = (IPollableChannel) context.GetObject("output2");
            IMessage reply = output2.Receive(TimeSpan.Zero);
            Assert.That(reply, Is.EqualTo(message));
        }
    }
}