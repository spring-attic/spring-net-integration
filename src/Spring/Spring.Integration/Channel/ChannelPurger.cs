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

using System;
using System.Collections;
using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Integration.Selector;
using Spring.Util;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A utility class for purging {@link Message Messages} from one or more
    /// {@link PollableChannel PollableChannels}. Any message that does <em>not</em>
    /// match the provided {@link MessageSelector} will be removed from the channel.
    /// If no {@link MessageSelector} is provided, then <em>all</em> messages will be
    /// cleared from the channel.
    /// <p>
    /// Note that the {@link #purge()} method operates on a snapshot of the messages
    /// within a channel at the time that the method is invoked. It is therefore
    /// possible that new messages will arrive on the channel during the purge
    /// operation and thus will <em>not</em> be removed. Likewise, messages to be
    /// purged may have been removed from the channel while the operation is taking
    /// place. Such messages will not be included in the returned list.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class ChannelPurger {

        private readonly IPollableChannel[] _channels;

        private readonly IMessageSelector _selector;

        public ChannelPurger(params IPollableChannel[] channels)
            : this((IMessageSelector)null, channels) {
        }

        public ChannelPurger(MessageSelectorAccept accept, params IPollableChannel[] channels)
            : this(new MessageSelector(accept), channels) {
        }

        public ChannelPurger(IMessageSelector selector, params IPollableChannel[] channels) {
            AssertUtils.ArgumentHasElements(channels, "channel");
            if(channels.Length == 1) {
                if (channels[0] == null)
                    throw new ArgumentException("channel must not be null");
            }
            _selector = selector;
            _channels = channels;
        }

        public IList<IMessage> Purge() {
            List<IMessage> purgedMessages = new List<IMessage>();
            foreach(IPollableChannel channel in _channels) {
                IList<IMessage> results = (_selector == null) ? channel.Clear() : channel.Purge(_selector);
                if(results != null) {
                    purgedMessages.AddRange(results);
                }
            }
            return purgedMessages;
        }
    }
}
