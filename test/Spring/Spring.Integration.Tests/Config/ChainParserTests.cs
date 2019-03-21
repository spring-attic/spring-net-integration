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
using Spring.Testing.NUnit;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    //@ContextConfiguration
    public class ChainParserTests : AbstractSpringContextTests
    {
        //@Autowired
        //@Qualifier("filterInput")
        //private MessageChannel filterInput;

        //@Autowired
        //@Qualifier("pollableInput")
        //private MessageChannel pollableInput;

        //@Autowired
        //@Qualifier("headerEnricherInput")
        //private MessageChannel headerEnricherInput;

        //@Autowired
        //@Qualifier("output")
        //private PollableChannel output;

        //@Autowired
        //@Qualifier("replyOutput")
        //private PollableChannel replyOutput;


        [Test]
        public void chainWithAcceptingFilter()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\ChainParserTests-context.xml");
            IMessage message = MessageBuilder.WithPayload("test").Build();

            IMessageChannel filterInput = (IMessageChannel) ctx.GetObject("filterInput");
            filterInput.Send(message);

            IPollableChannel output = (IPollableChannel) ctx.GetObject("output");
            IMessage reply = output.Receive(TimeSpan.Zero);

            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("foo"));
        }

        [Test]
        public void chainWithRejectingFilter()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\ChainParserTests-context.xml");
            IMessage message = MessageBuilder.WithPayload(123).Build();
            IMessageChannel filterInput = (IMessageChannel) ctx.GetObject("filterInput");
            filterInput.Send(message);

            IPollableChannel output = (IPollableChannel) ctx.GetObject("output");
            IMessage reply = output.Receive(TimeSpan.Zero);
            Assert.IsNull(reply);
        }

        [Test]
        public void chainWithHeaderEnricher()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\ChainParserTests-context.xml");
            IMessage message = MessageBuilder.WithPayload(123).Build();

            IMessageChannel headerEnricherInput = (IMessageChannel) ctx.GetObject("headerEnricherInput");
            headerEnricherInput.Send(message);
            IPollableChannel replyOutput = (IPollableChannel) ctx.GetObject("replyOutput");
            IMessage reply = replyOutput.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);

            Assert.That(reply.Payload, Is.EqualTo("foo"));
            Assert.That(reply.Headers.CorrelationId, Is.EqualTo("ABC"));
            Assert.That(reply.Headers.Get("testValue"), Is.EqualTo("XYZ"));
            Assert.That(reply.Headers.Get("testRef"), Is.EqualTo(123));
        }

        [Test]
        public void chainWithPollableInput()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\ChainParserTests-context.xml");
            IMessage message = MessageBuilder.WithPayload("test").Build();

            IMessageChannel pollableInput = (IMessageChannel) ctx.GetObject("pollableInput");
            pollableInput.Send(message);
            IPollableChannel output = (IPollableChannel) ctx.GetObject("output");
            IMessage reply = output.Receive(TimeSpan.FromMilliseconds(3000));

            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("foo"));
        }
    }
}