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
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Tests.Gateway;
using Spring.Integration.Tests.Util;
using Spring.Threading.Execution;

#endregion

namespace Spring.Integration.Tests.Config.Xml
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class GatewayParserTests
    {
        [Test]
        public void TestOneWay()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\gatewayParserTests.xml");
            ITestService service = (ITestService) context.GetObject("oneWay");
            service.OneWay("foo");
            IPollableChannel channel = (IPollableChannel) context.GetObject("requestChannel");
            IMessage result = channel.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.That(result.Payload, Is.EqualTo("foo"));
        }

        [Test]
        public void TestSolicitResponse()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\gatewayParserTests.xml");
            IPollableChannel channel = (IPollableChannel) context.GetObject("replyChannel");
            channel.Send(new StringMessage("foo"));
            ITestService service = (ITestService) context.GetObject("solicitResponse");
            string result = service.SolicitResponse();
            Assert.That(result, Is.EqualTo("foo"));
        }

        [Test, Ignore("does not work, have to investigate")]
        public void TestRequestReply()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\gatewayParserTests.xml");
            IPollableChannel requestChannel = (IPollableChannel) context.GetObject("requestChannel");
            IMessageChannel replyChannel = (IMessageChannel) context.GetObject("replyChannel");
            StartResponder(requestChannel, replyChannel);
            ITestService service = (ITestService) context.GetObject("requestReply");
            string result = service.RequestReply("foo");
            Assert.That(result, Is.EqualTo("foo"));
        }

        private static void StartResponder(IPollableChannel requestChannel, IMessageChannel replyChannel)
        {
            Executors.NewSingleThreadExecutor().Execute(delegate
                                                            {
                                                                IMessage request = requestChannel.Receive();
                                                                IMessage reply =
                                                                    MessageBuilder.FromMessage(request).SetCorrelationId
                                                                        (request.Headers.Id).Build();
                                                                replyChannel.Send(reply);
                                                            });
        }
    }
}