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
using System.Threading;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Gateway;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;
using Spring.Threading;
using Spring.Threading.Execution;

#endregion

namespace Spring.Integration.Tests.Gateway
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class GatewayProxyFactoryObjectTests
    {
        [Test]
        public void TestRequestReplyWithAnonymousChannel()
        {
            QueueChannel requestChannel = new QueueChannel();
            StartResponder(requestChannel);
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.DefaultRequestChannel = requestChannel;
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();
            String result = service.RequestReply("foo");
            Assert.That(result, Is.EqualTo("foobar"));
        }

        [Test]
        public void TestOneWay()
        {
            QueueChannel requestChannel = new QueueChannel();
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.DefaultRequestChannel = requestChannel;
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();
            service.OneWay("test");
            IMessage message = requestChannel.Receive(TimeSpan.FromMilliseconds(1000));
            Assert.IsNotNull(message);
            Assert.That(message.Payload, Is.EqualTo("test"));
        }

        [Test]
        public void TestSolicitResponse()
        {
            QueueChannel replyChannel = new QueueChannel();
            replyChannel.Send(new StringMessage("foo"));
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.DefaultRequestChannel = new DirectChannel();
            proxyFactory.DefaultReplyChannel = replyChannel;
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();

            string result = service.SolicitResponse();
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("foo"));
        }

        [Test]
        public void TestRequestReplyWithTypeConversion()
        {
            QueueChannel requestChannel = new QueueChannel();
            new Thread(new ThreadStart(delegate
                                           {
                                               IMessage input = requestChannel.Receive();
                                               StringMessage reply = new StringMessage(input.Payload + "456");
                                               ((IMessageChannel) input.Headers.ReplyChannel).Send(reply);
                                           })).Start();
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.DefaultRequestChannel = requestChannel;
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();
            int result = service.RequestReplyWithIntegers(123);
            Assert.That(result, Is.EqualTo(123456));
        }

        [Test]
        public void TestRequestReplyWithRendezvousChannelInApplicationContext()
        {
            IApplicationContext context = TestUtils.GetContext(@"Gateway\gatewayWithRendezvousChannel.xml");
            ITestService service = (ITestService) context.GetObject("proxy");
            string result = service.RequestReply("foo");
            Assert.That(result, Is.EqualTo("foo!!!"));
        }

        [Test, Ignore("fails")]
        public void TestRequestReplyWithResponseCorrelatorInApplicationContext()
        {
            IApplicationContext context = TestUtils.GetContext(@"Gateway\gatewayWithResponseCorrelator.xml");
            ITestService service = (ITestService) context.GetObject("proxy");
            string result = service.RequestReply("foo");
            Assert.That(result, Is.EqualTo("foo!!!"));
            TestChannelInterceptor interceptor = (TestChannelInterceptor) context.GetObject("interceptor");
            Assert.That(interceptor.SentCount, Is.EqualTo(1));
            Assert.That(interceptor.ReceivedCount, Is.EqualTo(1));
        }

        [Test, Ignore("fails")]
        public void TestMultipleMessagesWithResponseCorrelator()
        {
            IApplicationContext context = TestUtils.GetContext(@"Gateway\gatewayWithResponseCorrelator.xml");
            const int numRequests = 500;
            ITestService service = (ITestService) context.GetObject("proxy");
            string[] results = new string[numRequests];
            CountDownLatch latch = new CountDownLatch(numRequests);
            IExecutor executor = Executors.NewFixedThreadPool(numRequests);
            for (int i = 0; i < numRequests; i++)
            {
                int count = i;
                executor.Execute(delegate
                                     {
                                         // add some randomness to the ordering of requests
                                         try
                                         {
                                             Thread.Sleep(new Random().Next(100));
                                         }
                                         catch (ThreadInterruptedException e)
                                         {
                                             // ignore
                                         }
                                         results[count] = service.RequestReply("test-" + count);
                                         latch.CountDown();
                                     });
            }
            latch.Await(TimeSpan.FromSeconds(10));
            for (int i = 0; i < numRequests; i++)
            {
                Assert.That(results[i], Is.EqualTo("test-" + i + "!!!"));
            }
            TestChannelInterceptor interceptor = (TestChannelInterceptor) context.GetObject("interceptor");
            Assert.That(interceptor.SentCount, Is.EqualTo(numRequests));
            Assert.That(interceptor.ReceivedCount, Is.EqualTo(numRequests));
        }

        [Test]
        public void TestMessageAsMethodArgument()
        {
            QueueChannel requestChannel = new QueueChannel();
            StartResponder(requestChannel);
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.DefaultRequestChannel = requestChannel;
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();
            String result = service.RequestReplyWithMessageParameter(new StringMessage("foo"));
            Assert.That(result, Is.EqualTo("foobar"));
        }

        [Test]
        public void TestMessageAsReturnValue()
        {
            QueueChannel requestChannel = new QueueChannel();
            new Thread(new ThreadStart(delegate
                                           {
                                               IMessage input = requestChannel.Receive();
                                               StringMessage reply = new StringMessage(input.Payload + "bar");
                                               ((IMessageChannel) input.Headers.ReplyChannel).Send(reply);
                                           })).Start();
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.DefaultRequestChannel = requestChannel;
            proxyFactory.AfterPropertiesSet();
            ITestService service = (ITestService) proxyFactory.GetObject();
            IMessage result = service.RequestReplyWithMessageReturnValue("foo");
            Assert.That(result.Payload, Is.EqualTo("foobar"));
        }

        [Test]
        public void TestServiceMustBeInterface()
        {
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            int count = 0;
            try
            {
                proxyFactory.ServiceInterface = typeof (ITestService);
                count++;
                proxyFactory.ServiceInterface = typeof (string);
                count++;
            }
            catch (ArgumentException)
            {
                // expected
            }
            Assert.That(count, Is.EqualTo(1));
        }

        [Test, Ignore("check ToString()")]
        public void TestProxiedToStringMethod()
        {
            GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
            proxyFactory.DefaultRequestChannel = new DirectChannel();
            proxyFactory.ServiceInterface = typeof (ITestService);
            proxyFactory.AfterPropertiesSet();
            Object proxy = proxyFactory.GetObject();
            const string expected = "gateway proxy for";
            string current = proxy.ToString();
            Assert.That(current.Substring(0, expected.Length), Is.EqualTo(expected));
        }

        // TODO public void TestCheckedExceptionRethrownAsIs
        //[Test, ExpectedException(typeof(TestException))]
        //public void TestCheckedExceptionRethrownAsIs() {
        //    GatewayProxyFactoryObject proxyFactory = new GatewayProxyFactoryObject();
        //    DirectChannel channel = new DirectChannel();
        //    EventDrivenConsumer consumer = new EventDrivenConsumer(channel, new MessageHandler() {
        //        public void handleMessage(Message<?> message) {
        //            MethodInfo method = ReflectionUtils.GetMethod(typeof(GatewayProxyFactoryObjectTests), "throwTestException",null);
        //            method.Invoke(this, null);            
        //        }
        //    });
        //    consumer.start();
        //    proxyFactory.setDefaultRequestChannel(channel);
        //    proxyFactory.setServiceInterface(TestExceptionThrowingInterface.class);
        //    proxyFactory.afterPropertiesSet();
        //    TestExceptionThrowingInterface proxy = (TestExceptionThrowingInterface) proxyFactory.getObject();
        //    proxy.throwCheckedException("test");
        //}


        private static void StartResponder(IPollableChannel requestChannel)
        {
            new Thread(new ThreadStart(delegate
                                           {
                                               IMessage input = requestChannel.Receive();
                                               StringMessage reply = new StringMessage(input.Payload + "bar");
                                               ((IMessageChannel) input.Headers.ReplyChannel).Send(reply);
                                           })).Start();
        }

        public static void ThrowTestException()
        {
            throw new TestException();
        }


        private interface ITestExceptionThrowingInterface
        {
            string ThrowCheckedException(string s);
        }


        private class TestException : Exception
        {
        }
    }
}