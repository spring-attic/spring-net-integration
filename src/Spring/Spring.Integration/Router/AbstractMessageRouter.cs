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

using System;
using System.Collections.Generic;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;

namespace Spring.Integration.Router {
    /// <summary>
    /// Base class for Message Routers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractMessageRouter : AbstractMessageHandler {

        private volatile IMessageChannel _defaultOutputChannel;

        private volatile bool _resolutionRequired;

        private MessageChannelTemplate _channelTemplate = new MessageChannelTemplate();

        /// <summary>
        /// Set the default channel where Messages should be sent if channel
        /// resolution fails to return any channels. If no default channel is
        /// provided, the router will either drop the Message or throw an Exception
        /// depending on the value of {@link #resolutionRequired}. 
        /// </summary>
        public IMessageChannel DefaultOutputChannel {
            set { _defaultOutputChannel = value; }
        }

        /// <summary>
        /// Set the timeout for sending a message to the resolved channel. By
        /// default, there is no timeout, meaning the send will block indefinitely.
        /// </summary>
        public TimeSpan Timeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        /// <summary>
        /// Set whether this router should always be required to resolve at least one
        /// channel. The default is 'false'. To trigger an exception whenever the
        /// resolver returns null or an empty channel list, and this endpoint has 
        /// no 'defaultOutputChannel' configured, set this value to 'true'.
        /// </summary>
        public bool ResolutionRequired {
            set { _resolutionRequired = value; }
        }

        protected override void HandleMessageInternal(IMessage message) {
            bool sent = false;
            ICollection<IMessageChannel> results = DetermineTargetChannels(message);
            if(results != null) {
                foreach(IMessageChannel channel in results) {
                    if(channel != null) {
                        if(_channelTemplate.Send(message, channel)) {
                            sent = true;
                        }
                    }
                }
            }
            if(!sent) {
                if(_defaultOutputChannel != null) {
                    sent = _channelTemplate.Send(message, _defaultOutputChannel);
                }
                else if(_resolutionRequired) {
                    throw new MessageDeliveryException(message, "no channel resolved by router and no default output channel defined");
                }
            }
        }

        /**
         * Subclasses must implement this method to return the target channels for
         * a given Message.
         */
        protected abstract ICollection<IMessageChannel> DetermineTargetChannels(IMessage message);
    }
}