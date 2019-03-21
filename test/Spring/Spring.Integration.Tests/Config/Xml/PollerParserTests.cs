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
using Spring.Integration.Context;
using Spring.Integration.Scheduling;
using Spring.Integration.Tests.Util;
using Spring.Objects.Factory;

#endregion

namespace Spring.Integration.Tests.Config.Xml
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class PollerParserTests
    {
        [Test]
        public void DefaultPollerWithId()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\defaultPollerWithId.xml");
            object poller = context.GetObject("defaultPollerWithId");
            Assert.IsNotNull(poller);
            object defaultPoller = context.GetObject(IntegrationContextUtils.DefaultPollerMetadataObjectName);
            Assert.IsNotNull(defaultPoller);
            Assert.That(context.GetObject("defaultPollerWithId"), Is.EqualTo(defaultPoller));
        }

        [Test]
        public void DefaultPollerWithoutId()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\defaultPollerWithoutId.xml");
            object defaultPoller = context.GetObject(IntegrationContextUtils.DefaultPollerMetadataObjectName);
            Assert.IsNotNull(defaultPoller);
        }

        [Test, ExpectedException(typeof (ObjectDefinitionStoreException))]
        public void MultipleDefaultPollers()
        {
            TestUtils.GetContext(@"Config\Xml\multipleDefaultPollers.xml");
        }

        [Test, ExpectedException(typeof (ObjectDefinitionStoreException))]
        public void TopLevelPollerWithoutId()
        {
            TestUtils.GetContext(@"Config\Xml\topLevelPollerWithoutId.xml");
        }

        [Test]
        public void PollerWithAdviceChain()
        {
            IApplicationContext context = TestUtils.GetContext(@"Config\Xml\pollerWithAdviceChain.xml");
            Object poller = context.GetObject("poller");
            Assert.IsNotNull(poller);
            PollerMetadata metadata = (PollerMetadata) poller;
            Assert.IsNotNull(metadata.AdviceChain);
            Assert.That(metadata.AdviceChain.Count, Is.EqualTo(3));
            Assert.That(metadata.AdviceChain[0], Is.EqualTo(context.GetObject("adviceBean1")));
            Assert.That(metadata.AdviceChain[1].GetType(), Is.EqualTo(typeof (TestAdviceObject)));
            Assert.That(((TestAdviceObject) metadata.AdviceChain[1]).Id, Is.EqualTo(2));
            Assert.That(metadata.AdviceChain[2], Is.EqualTo(context.GetObject("adviceBean3")));
        }
    }
}