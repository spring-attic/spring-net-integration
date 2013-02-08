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
using Spring.Integration.Util;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class TestChannelResolver : IChannelResolver
    {
        private volatile IDictionary<string, IMessageChannel> channels = new Dictionary<string, IMessageChannel>();
                                                              // TODO new ConcurrentDictionary<string, IMessageChannel>();

        public IMessageChannel ResolveChannelName(string channelName)
        {
            return DictionaryUtils.Get(channels, channelName);
        }

        //@Autowired
        public IDictionary<string, IMessageChannel> Channels
        {
            set { this.channels = channels; }
        }

        public void AddChannel(IMessageChannel channel)
        {
            Assert.IsNotNull(channel, "'channel' must not be null");
            Assert.IsNotNull(channel.Name, "channel name must not be null");
            DictionaryUtils.Put(channels, channel.Name, channel);
        }
    }
}