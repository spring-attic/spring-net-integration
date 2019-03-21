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
using System.Threading;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Dispatcher;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;
using Spring.Integration.Util;
using Spring.Threading;
using Spring.Threading.Execution;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class PublishSubscribeChannelParserTests
    {
        [Test]
        public void DefaultChannel()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\PublishSubscribeChannelParserTests.xml");
            PublishSubscribeChannel channel = (PublishSubscribeChannel) ctx.GetObject("defaultChannel");

            BroadcastingDispatcher dispatcher = TestUtils.GetPropertyValue<BroadcastingDispatcher>(channel, "Dispatcher");
            IExecutor taskExecutor = TestUtils.GetPropertyValue<IExecutor>(dispatcher, "TaskExecutor");
            Assert.IsNull(taskExecutor);
            Assert.IsFalse(TestUtils.GetFieldValue<bool>(dispatcher, "_applySequence"));
        }

        [Test]
        public void applySequenceEnabled()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\PublishSubscribeChannelParserTests.xml");
            PublishSubscribeChannel channel = (PublishSubscribeChannel) ctx.GetObject("channelWithApplySequenceEnabled");

            BroadcastingDispatcher dispatcher = TestUtils.GetPropertyValue<BroadcastingDispatcher>(channel, "Dispatcher");
            Assert.IsTrue(TestUtils.GetFieldValue<bool>(dispatcher, "_applySequence"));
        }

        [Test]
        public void channelWithTaskExecutor()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\PublishSubscribeChannelParserTests.xml");
            PublishSubscribeChannel channel = (PublishSubscribeChannel) ctx.GetObject("channelWithTaskExecutor");

            IExecutor executor = TestUtils.GetPropertyValue<IExecutor>(channel, "Dispatcher.TaskExecutor");
            Assert.IsNotNull(executor);
            Assert.That(executor.GetType(), Is.EqualTo(typeof (ErrorHandlingTaskExecutor)));
            IExecutor innerExecutor = TestUtils.GetFieldValue<IExecutor>(executor, "_taskExecutor");
            Assert.That(innerExecutor, Is.EqualTo(ctx.GetObject("pool")));
        }

        [Test]
        public void channelWithErrorHandler()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\PublishSubscribeChannelParserTests.xml");
            PublishSubscribeChannel channel = (PublishSubscribeChannel) ctx.GetObject("channelWithErrorHandler");

            IErrorHandler errorHandler = TestUtils.GetFieldValue<IErrorHandler>(channel, "_errorHandler");
            Assert.IsNotNull(errorHandler);
            Assert.That(errorHandler, Is.EqualTo(ctx.GetObject("testErrorHandler")));
        }

        [Test]
        public void UsingPublishSubscribeChannels()
        {
            PublishSubscribeChannel channel = new PublishSubscribeChannel();

            channel.Subscribe(new SimpleMessageHandler("Handler-1"));
            channel.Subscribe(new SimpleMessageHandler("Handler-2"));

            IMessage diningMessage =
                MessageBuilder.WithPayload("Hello World").Build();

            channel.Send(diningMessage);
            channel.Send(diningMessage);
            channel.Send(diningMessage);

        }
    }

    public class SimpleMessageHandler : IMessageHandler
    {
        private string name;
        public SimpleMessageHandler(string name)
        {
            this.name = name;
        }

        #region Implementation of IMessageHandler

        public void HandleMessage(IMessage message)
        {
            Console.WriteLine(string.Format("Thread Id [{0}], Handler [{1}], Received dining message [{2}]",
                                            Thread.CurrentThread.ManagedThreadId, name, message.Payload));
        }

        #endregion
    }

    public class ThreadPerTaskExecutor : IExecutor
    {
        public void Execute(IRunnable r)
        {
            new Thread(new ThreadStart(r.Run)).Start();
        }

        public void Execute(Action action)
        {
            new Thread(new ThreadStart(action)).Start();
        }

    }
}