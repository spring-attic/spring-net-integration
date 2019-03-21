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
using System.Collections;
using System.Collections.Generic;
using Spring.Integration.Channel;
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// A composite {@link MessageHandler} implementation that invokes a chain of
    /// MessageHandler instances in order.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageHandlerChain : IntegrationObjectSupport, IMessageHandler {

        private volatile IList<IMessageHandler> _handlers;

        private volatile IMessageChannel _outputChannel;

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();

        /// <summary>
        /// set the list of handlers
        /// </summary>
        //public IList<IMessageHandler> Handlers {
        public IList<IMessageHandler> Handlers {
            set {
                _handlers = value;
            }
        }

        /// <summary>
        /// set the output channel
        /// </summary>
        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        /// <summary>
        /// implementation of <see cref="IInitializingObject.AfterPropertiesSet"/>
        /// </summary>
        public void AfterPropertiesSet() {
            lock(_initializationMonitor) {
                if(!_initialized) {
                    if(_handlers == null || _handlers.Count == 0)
                        throw new ArgumentException("handler list must not be empty");
                    ConfigureChain();
                    _initialized = true;
                }
            }
        }

        public void HandleMessage(IMessage message) {
            if(!_initialized) {
                AfterPropertiesSet();
            }
            _handlers[0].HandleMessage(message);
        }

        private void ConfigureChain() {
            DirectChannel channel = null;
            IList<IMessageHandler> handlers = _handlers;
            for(int i = 0; i < handlers.Count; i++) {
                bool first = (i == 0);
                bool last = (i == handlers.Count - 1);
                IMessageHandler handler = handlers[i];
                if(!first) {
                    EventDrivenConsumer consumer = new EventDrivenConsumer(channel, handler);
                    consumer.Start();
                }
                if(!last) {
                    if(!(handler is AbstractReplyProducingMessageHandler))
                        throw new ArgumentException("All handlers except for the last one in the chain must implement " + typeof(AbstractReplyProducingMessageHandler).Name);
                    channel = new DirectChannel();
                    channel.ObjectName = "_" + this + ".channel#" + i;
                    ((AbstractReplyProducingMessageHandler)handler).OutputChannel = channel;
                }
                else if(handler is AbstractReplyProducingMessageHandler) {
                    IMessageChannel replyChannel = (_outputChannel != null) ? _outputChannel : new ReplyForwardingMessageChannel(this);
                    ((AbstractReplyProducingMessageHandler)handler).OutputChannel = replyChannel;
                }
                else {
                    if(_outputChannel == null)
                        throw new ArgumentException("An output channel was provided, but the final handler in the chain is not an "
                                + "instance of [" + typeof(AbstractReplyProducingMessageHandler).Name + "]");
                }
            }
        }


        private class ReplyForwardingMessageChannel : IMessageChannel {
            private readonly MessageHandlerChain _outer;

            public ReplyForwardingMessageChannel(MessageHandlerChain outer) {
                _outer = outer;
            }

            public string Name {
                get {
                    return _outer.ObjectName;
                }
            }

            public bool Send(IMessage message) {
                return Send(message, TimeSpan.FromMilliseconds(-1));
            }

            public bool Send(IMessage message, TimeSpan timeout) {
                object replyChannelHeader = message.Headers.ReplyChannel;
                if(replyChannelHeader == null) {
                    throw new MessageHandlingException(message, "no replyChannel header available");
                }
                IMessageChannel replyChannel;
                if(replyChannelHeader is IMessageChannel) {
                    replyChannel = (IMessageChannel)replyChannelHeader;
                }
                else if(replyChannelHeader is string) {
                    AssertUtils.ArgumentNotNull(_outer.ChannelResolver, "ChannelResolver is required");
                    replyChannel = _outer.ChannelResolver.ResolveChannelName((string)replyChannelHeader);
                }
                else {
                    throw new MessageHandlingException(message, "invalid replyChannel type [" + replyChannelHeader.GetType() + "]");
                }
                return (timeout.TotalMilliseconds >= 0) ? replyChannel.Send(message, timeout) : replyChannel.Send(message);
            }
        }
    }
}
