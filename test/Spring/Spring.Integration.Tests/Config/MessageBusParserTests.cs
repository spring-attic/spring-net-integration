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

using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Context;
using Spring.Integration.Scheduling;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class MessageBusParserTests
    {
        [Test]
        public void testErrorChannelReference()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\messageBusWithErrorChannel.xml");
            ObjectFactoryChannelResolver resolver = new ObjectFactoryChannelResolver(ctx);
            Assert.That(resolver.ResolveChannelName("errorChannel"), Is.EqualTo(ctx.GetObject("errorChannel")));
        }

        [Test]
        public void testDefaultErrorChannel()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\messageBusWithDefaults.xml");
            ObjectFactoryChannelResolver resolver = new ObjectFactoryChannelResolver(ctx);
            Assert.That(resolver.ResolveChannelName("errorChannel"), Is.EqualTo(ctx.GetObject("errorChannel")));
        }

        [Test, Ignore("investigate SimpleApplicationEventMulticaster")]
        public void testMulticasterIsSyncByDefault()
        {
            //IApplicationContext ctx = TestUtils.GetContext(@"Config\messageBusWithDefaults.xml");
            //SimpleApplicationEventMulticaster multicaster = (SimpleApplicationEventMulticaster)
            //        context.getBean(AbstractApplicationContext.APPLICATION_EVENT_MULTICASTER_BEAN_NAME);
            //DirectFieldAccessor accessor = new DirectFieldAccessor(multicaster);
            //object taskExecutor = accessor.getPropertyValue("taskExecutor");
            //assertEquals(SyncTaskExecutor.class, taskExecutor.getClass());
        }

        [Test, Ignore("investigate SimpleApplicationEventMulticaster")]
        public void testAsyncMulticasterExplicitlySetToFalse()
        {
            //AbstractApplicationContext context = new ClassPathXmlApplicationContext(
            //        "messageBusWithoutAsyncEventMulticaster.xml", this.getClass());
            //context.refresh();
            //SimpleApplicationEventMulticaster multicaster = (SimpleApplicationEventMulticaster)
            //        context.getBean(AbstractApplicationContext.APPLICATION_EVENT_MULTICASTER_BEAN_NAME);
            //DirectFieldAccessor accessor = new DirectFieldAccessor(multicaster);
            //Object taskExecutor = accessor.getPropertyValue("taskExecutor");
            //assertEquals(SyncTaskExecutor.class, taskExecutor.getClass());
        }

        [Test, Ignore("investigate SimpleApplicationEventMulticaster")]
        public void testAsyncMulticaster()
        {
            //    AbstractApplicationContext context = new ClassPathXmlApplicationContext(
            //            "messageBusWithAsyncEventMulticaster.xml", this.getClass());
            //    context.refresh();
            //    SimpleApplicationEventMulticaster multicaster = (SimpleApplicationEventMulticaster)
            //            context.getBean(AbstractApplicationContext.APPLICATION_EVENT_MULTICASTER_BEAN_NAME);
            //    DirectFieldAccessor accessor = new DirectFieldAccessor(multicaster);
            //    Object taskExecutor = accessor.getPropertyValue("taskExecutor");
            //    assertEquals(ThreadPoolTaskExecutor.class, taskExecutor.getClass());
        }

        [Test]
        public void testExplicitlyDefinedTaskSchedulerHasCorrectType()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\messageBusWithTaskScheduler.xml");
            ITaskScheduler scheduler = (ITaskScheduler) ctx.GetObject("taskScheduler");
            Assert.That(scheduler.GetType(), Is.EqualTo(typeof (StubTaskScheduler)));
        }

        [Test]
        public void testExplicitlyDefinedTaskSchedulerMatchesUtilLookup()
        {
            IApplicationContext ctx = TestUtils.GetContext(@"Config\messageBusWithTaskScheduler.xml");
            ITaskScheduler scheduler = (ITaskScheduler) ctx.GetObject("taskScheduler");
            Assert.That(IntegrationContextUtils.GetTaskScheduler(ctx), Is.EqualTo(scheduler));
        }
    }
}