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
using Spring.Integration.Selector;
using Spring.Integration.Tests.Util;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    //@ContextConfiguration
    public class FilterParserTests
    {
        //@Autowired @Qualifier("adapterInput")
        //IMessageChannel adapterInput;

        //@Autowired @Qualifier("adapterOutput")
        //IPollableChannel adapterOutput;

        //@Autowired @Qualifier("implementationInput")
        //IMessageChannel implementationInput;

        //@Autowired @Qualifier("implementationOutput")
        //IPollableChannel implementationOutput;

        //@Autowired @Qualifier("exceptionInput")
        //IMessageChannel exceptionInput;


        [Test]
        public void filterWithSelectorAdapterAccepts()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel adapterInput = (IMessageChannel) ctx.GetObject("adapterInput");
            adapterInput.Send(new StringMessage("test"));

            IPollableChannel adapterOutput = (IPollableChannel) ctx.GetObject("adapterOutput");
            IMessage reply = adapterOutput.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void filterWithSelectorAdapterRejects()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel adapterInput = (IMessageChannel) ctx.GetObject("adapterInput");
            adapterInput.Send(new StringMessage(""));
            IPollableChannel adapterOutput = (IPollableChannel) ctx.GetObject("adapterOutput");
            IMessage reply = adapterOutput.Receive(TimeSpan.Zero);
            Assert.IsNull(reply);
        }

        [Test]
        public void filterWithSelectorImplementationAccepts()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel implementationInput = (IMessageChannel) ctx.GetObject("implementationInput");
            implementationInput.Send(new StringMessage("test"));
            IPollableChannel implementationOutput = (IPollableChannel) ctx.GetObject("implementationOutput");
            IMessage reply = implementationOutput.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void filterWithSelectorImplementationRejects()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel implementationInput = (IMessageChannel) ctx.GetObject("implementationInput");
            implementationInput.Send(new StringMessage(""));
            IPollableChannel implementationOutput = (IPollableChannel) ctx.GetObject("implementationOutput");
            IMessage reply = implementationOutput.Receive(TimeSpan.Zero);
            Assert.IsNull(reply);
        }

        [Test]
        public void exceptionThrowingFilterAccepts()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel exceptionInput = (IMessageChannel) ctx.GetObject("exceptionInput");
            exceptionInput.Send(new StringMessage("test"));
            IPollableChannel implementationOutput = (IPollableChannel) ctx.GetObject("implementationOutput");
            IMessage reply = implementationOutput.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
        }

        [Test, ExpectedException(typeof (MessageRejectedException))]
        public void exceptionThrowingFilterRejects()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\FilterParserTests-context.xml");
            IMessageChannel exceptionInput = (IMessageChannel) ctx.GetObject("exceptionInput");
            exceptionInput.Send(new StringMessage(""));
        }


        public class TestSelectorBean
        {
            public bool HasText(string s)
            {
                return StringUtils.HasText(s);
            }
        }


        public class TestSelectorImpl : IMessageSelector
        {
            public bool Accept(IMessage message)
            {
                if (message != null && message.Payload is string)
                {
                    return StringUtils.HasText((string) message.Payload);
                }
                return false;
            }
        }
    }
}