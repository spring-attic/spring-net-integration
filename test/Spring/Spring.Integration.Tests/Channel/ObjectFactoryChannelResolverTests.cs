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
using Spring.Context.Support;
using Spring.Integration.Channel;
using Spring.Integration.Core;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class ObjectFactoryChannelResolverTests
    {
        [Test]
        public void LookupRegisteredChannel()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            QueueChannel testChannel = new QueueChannel();
            testChannel.ObjectName = "testChannel";
            context.ObjectFactory.RegisterSingleton("testChannel", testChannel);

            ObjectFactoryChannelResolver resolver = new ObjectFactoryChannelResolver(context);
            IMessageChannel lookedUpChannel = resolver.ResolveChannelName("testChannel");
            Assert.IsNotNull(testChannel);
            Assert.That(lookedUpChannel, Is.SameAs(testChannel));
        }

        [Test, ExpectedException(typeof (ChannelResolutionException))]
        public void lookupNonRegisteredChannel()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            ObjectFactoryChannelResolver resolver = new ObjectFactoryChannelResolver(context);
            resolver.ResolveChannelName("noSuchChannel");
        }
    }
}