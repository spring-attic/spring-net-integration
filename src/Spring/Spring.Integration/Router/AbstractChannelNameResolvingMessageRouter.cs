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
using System.Collections.Generic;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Router {
    /// <summary>
    /// A base class for router implementations that return only
    /// the channel name(s) rather than {@link MessageChannel} instances.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class AbstractChannelNameResolvingMessageRouter : AbstractMessageRouter, IObjectFactoryAware, IInitializingObject {

        private volatile IChannelResolver _channelResolver;

        private volatile string _prefix;

        private volatile string _suffix;

        private volatile IObjectFactory _objectFactory;


        public IChannelResolver ChannelResolver {
            set { _channelResolver = value; }
        }

        public string Prefix {
            set { _prefix = value; }
        }

        public string Suffix {
            set { _suffix = value; }
        }

        public IObjectFactory ObjectFactory {
            set { _objectFactory = value; }
        }

        public void AfterPropertiesSet() {
            if(_channelResolver == null) {
                AssertUtils.ArgumentNotNull(_objectFactory, "either a ChannelResolver or BeanFactory is required");
                _channelResolver = new ObjectFactoryChannelResolver(_objectFactory);
            }
        }

        protected override ICollection<IMessageChannel> DetermineTargetChannels(IMessage message) {
            AfterPropertiesSet();
            ICollection<IMessageChannel> channels = new List<IMessageChannel>();
            string[] channelNames = DetermineTargetChannelNames(message);
            if(channelNames == null) {
                return null;
            }
            foreach(string channelName in channelNames) {
                if(channelName == null)
                    continue;
                if (_channelResolver == null)
                    throw new InvalidOperationException("unable to resolve channel names, no ChannelResolver available");
                
                string name = channelName;
                if(_prefix != null) {
                    name = _prefix + name;
                }
                if(_suffix != null) {
                    name = name + _suffix;
                }
                IMessageChannel channel = _channelResolver.ResolveChannelName(name);
                if(channel == null) {
                    throw new MessagingException(message, "failed to resolve channel name '" + name + "'");
                }
                channels.Add(channel);
            }
            return channels;
        }

        /**
         * Subclasses must implement this method to return the channel name(s).
         */
        protected abstract string[] DetermineTargetChannelNames(IMessage message);
    }
}
