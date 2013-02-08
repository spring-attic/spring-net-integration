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
using System.Reflection;
using Spring.Integration.Attributes;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Util;

namespace Spring.Integration.Router {
    /// <summary>
    /// A Message Router that invokes the specified method on the given object. The
    /// method's return value may be a single IMessageChannel instance, a single
    /// string to be interpreted as a channel name, or a Collection (or Array) of
    /// either type. If the method returns channel names, then a
    /// {@link ChannelResolver} is required.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MethodInvokingRouter : AbstractMessageRouter {

        private readonly MessageMappingMethodInvoker _invoker;

        private volatile IChannelResolver _channelResolver;


        public MethodInvokingRouter(object obj, MethodInfo method) {
            _invoker = new MessageMappingMethodInvoker(obj, method);
        }

        public MethodInvokingRouter(object obj, string methodName) {
            _invoker = new MessageMappingMethodInvoker(obj, methodName);
        }

        public MethodInvokingRouter(object obj) {
            _invoker = new MessageMappingMethodInvoker(obj, typeof(RouterAttribute));
        }

        /// <summary>
        /// Provide the {@link ChannelResolver} strategy to use for methods that
        /// return a channel name rather than a {@link IMessageChannel} instance.
        /// </summary>
        public IChannelResolver ChannelResolver {
            set { _channelResolver = value; }
        }

        protected override ICollection<IMessageChannel> DetermineTargetChannels(IMessage message) {
            object result = _invoker.InvokeMethod(message);
            if(result == null) {
                return null;
            }
            List<IMessageChannel> channels = new List<IMessageChannel>();
            if(result is string) {
                AddChannel(result, channels);
            }
            else if (result is IMessageChannel)
            {
                channels.Add((IMessageChannel)result);
            }
            else if(result is IEnumerable) {
                foreach(object obj in (IEnumerable)result) {
                    AddChannel(obj, channels);
                }
            }
            else if(result is IMessageChannel[]) {
                channels.AddRange((IMessageChannel[])result);
            }
            else if(result is string[]) {
                foreach(string channelName in (string[])result) {
                    AddChannel(channelName, channels);
                }
            }
            else {
                throw new InvalidOperationException("router method must return type 'IMessageChannel' or 'string'");
            }
            return channels;
        }

        private void AddChannel(object channelOrName, ICollection<IMessageChannel> channels) {
            if(channelOrName == null) {
                return;
            }
            if(channelOrName is IMessageChannel) {
                channels.Add((IMessageChannel)channelOrName);
            }
            else if(channelOrName is string) {
                string channelName = (string)channelOrName;
                AssertUtils.State(_channelResolver != null, "unable to resolve channel names, no ChannelResolver available");
                IMessageChannel channel = _channelResolver.ResolveChannelName(channelName);
                if(channel == null) {
                    throw new MessagingException("failed to resolve channel name '" + channelName + "'");
                }
                channels.Add(channel);
            }
            else {
                throw new MessagingException("unsupported return type for router [" + channelOrName.GetType() + "]");
            }
        }
    }
}
