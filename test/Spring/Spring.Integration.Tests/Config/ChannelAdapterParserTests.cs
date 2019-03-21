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
using Spring.Context.Support;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
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
    public class ChannelAdapterParserTests : AbstractSpringContextTests
    {
        private IApplicationContext _ctx;

        [SetUp]
        public void StartContext()
        {
            _ctx = TestUtils.GetContext(@"Config\ChannelAdapterParserTests-context.xml");

            ((AbstractApplicationContext) _ctx).Start();
        }

        [TearDown]
        public void StopContext()
        {
            ((AbstractApplicationContext) _ctx).Stop();
        }


        [Test]
        public void TargetOnly()
        {
            //TODO investigate TimeSpan issue when using BCL syntax.
            const string objectName = "outboundWithImplicitChannel";
            object channel = _ctx.GetObject(objectName);
            Assert.IsTrue(channel is DirectChannel);
            ObjectFactoryChannelResolver channelResolver = new ObjectFactoryChannelResolver(_ctx);
            Assert.IsNotNull(channelResolver.ResolveChannelName(objectName));
            object adapter = _ctx.GetObject(objectName + ".adapter");
            Assert.IsNotNull(adapter);
            Assert.IsTrue(adapter is EventDrivenConsumer);
            TestConsumer consumer = (TestConsumer) _ctx.GetObject("consumer");
            Assert.IsNull(consumer.LastMessage);
            IMessage message = new StringMessage("test");
            Assert.IsTrue(((IMessageChannel) channel).Send(message));
            Assert.IsNotNull(consumer.LastMessage);
            Assert.That(consumer.LastMessage, Is.EqualTo(message));
        }

        [Test]
        public void MethodInvokingConsumer()
        {
            const string objectName = "methodInvokingConsumer";
            object channel = _ctx.GetObject(objectName);
            Assert.IsTrue(channel is DirectChannel);
            ObjectFactoryChannelResolver channelResolver = new ObjectFactoryChannelResolver(_ctx);
            Assert.IsNotNull(channelResolver.ResolveChannelName(objectName));
            object adapter = _ctx.GetObject(objectName + ".adapter");
            Assert.IsNotNull(adapter);
            Assert.IsTrue(adapter is EventDrivenConsumer);
            TestObject testBean = (TestObject) _ctx.GetObject("testObject");
            Assert.IsNull(testBean.GetMessage());
            IMessage message = new StringMessage("consumer test");
            Assert.IsTrue(((IMessageChannel) channel).Send(message));
            Assert.IsNotNull(testBean.GetMessage());
            Assert.That(testBean.GetMessage(), Is.EqualTo("consumer test"));
        }

        [Test, Ignore("not yet finished")]
        public void MethodInvokingSource()
        {
            const string objectName = "methodInvokingSource";
            IPollableChannel channel = (IPollableChannel) _ctx.GetObject("queueChannel");
            TestObject testObject = (TestObject) _ctx.GetObject("testObject");
            testObject.Store("source test");
            object adapter = _ctx.GetObject(objectName);
            Assert.IsNotNull(adapter);
            Assert.IsTrue(adapter is SourcePollingChannelAdapter);
            ((SourcePollingChannelAdapter) adapter).Start();
            IMessage message = channel.Receive(TimeSpan.FromMilliseconds(10000));
            Assert.IsNotNull(message);
            Assert.That(testObject.GetMessage(), Is.EqualTo("source test"));
            ((SourcePollingChannelAdapter) adapter).Stop();
        }

        [Test, ExpectedException(typeof (ChannelResolutionException))]
        public void MethodInvokingSourceAdapterIsNotChannel()
        {
            ObjectFactoryChannelResolver channelResolver = new ObjectFactoryChannelResolver(_ctx);
            channelResolver.ResolveChannelName("methodInvokingSource");
        }
    }
}