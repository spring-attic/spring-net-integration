#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Config.Xml;
using Spring.Integration.Scheduling;
using Spring.Objects.Factory.Xml;
using Spring.Testing.NUnit;

namespace Spring.Integration.Nms.Config
{
    [TestFixture]
    public class DefaultConfigurationTests: AbstractDependencyInjectionSpringContextTests
    {
        protected override IConfigurableApplicationContext LoadContextLocations(string[] locations)
        {
            NamespaceParserRegistry.RegisterParser(typeof(IntegrationNamespaceParser));
            NamespaceParserRegistry.RegisterParser(typeof(NmsNamespaceParser));
            return base.LoadContextLocations(locations);
        }
        
        [Test]
        public void VerifyErrorChannel()
        {
            object errorChannel = this.applicationContext.GetObject("errorChannel");
            Assert.That(errorChannel, Is.Not.Null);
            Assert.That(errorChannel.GetType(), Is.EqualTo(typeof (PublishSubscribeChannel)));
        }

        [Test]
        public void VerifyNullChannel()
        {
            object nullChannel = this.applicationContext.GetObject("nullChannel");
            Assert.That(nullChannel, Is.Not.Null);
            Assert.That(nullChannel.GetType(), Is.EqualTo(typeof(NullChannel)));
        }

        [Test]
        public void VerifyTaskScheduler()
        {
            object taskScheduler = this.applicationContext.GetObject("taskScheduler");
            Assert.That(taskScheduler, Is.Not.Null);
            Assert.That(taskScheduler.GetType(), Is.EqualTo(typeof(SimpleTaskScheduler)));
        }

        #region Overrides of AbstractDependencyInjectionSpringContextTests

        protected override string[] ConfigLocations
        {
            get { return new string[] {@"Nms\Config\DefaultConfigurationTests.xml"}; }
        }

        #endregion
    }
}