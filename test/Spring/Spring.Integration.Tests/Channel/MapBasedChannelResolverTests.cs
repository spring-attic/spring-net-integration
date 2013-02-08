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

using System.Collections.Generic;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class MapBasedChannelResolverTests
    {
        [Test]
        public void mapContainsChannel()
        {
            IMessageChannel testChannel = new QueueChannel();
            IDictionary<string, IMessageChannel> channelMap = new Dictionary<string, IMessageChannel>();
            channelMap.Add("testChannel", testChannel);
            MapBasedChannelResolver resolver = new MapBasedChannelResolver();
            resolver.ChannelMap = channelMap;
            IMessageChannel result = resolver.ResolveChannelName("testChannel");
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo(testChannel));
        }

        [Test]
        public void mapDoesNotContainChannel()
        {
            IMessageChannel testChannel = new QueueChannel();
            IDictionary<string, IMessageChannel> channelMap = new Dictionary<string, IMessageChannel>();
            channelMap.Add("testChannel", testChannel);
            MapBasedChannelResolver resolver = new MapBasedChannelResolver();
            resolver.ChannelMap = channelMap;
            IMessageChannel result = resolver.ResolveChannelName("noSuchChannel");
            Assert.IsNull(result);
        }

        [Test]
        public void emptyMap()
        {
            IDictionary<string, IMessageChannel> channelMap = new Dictionary<string, IMessageChannel>();
            MapBasedChannelResolver resolver = new MapBasedChannelResolver();
            resolver.ChannelMap = channelMap;
            IMessageChannel result = resolver.ResolveChannelName("testChannel");
            Assert.IsNull(result);
        }

        [Test] //(expected = IllegalArgumentException.class)
        public void NullMapRejected()
        {
            MapBasedChannelResolver resolver = new MapBasedChannelResolver();
            resolver.ChannelMap = null;
        }
    }
}