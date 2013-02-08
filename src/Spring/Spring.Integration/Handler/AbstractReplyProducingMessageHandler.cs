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
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// Base class for MessageHandlers that are capable of producing replies.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class AbstractReplyProducingMessageHandler : AbstractMessageHandler, IObjectFactoryAware {

        public static TimeSpan DEFAULT_SEND_TIMEOUT = TimeSpan.FromMilliseconds(1000);

        private IMessageChannel _outputChannel;

        private volatile IChannelResolver _channelResolver;

        private volatile bool _requiresReply;

        private readonly MessageChannelTemplate _channelTemplate;


        /// <summary>
        /// create a new <see cref="AbstractReplyProducingMessageHandler"/> with a <see cref="MessageChannelTemplate"/> and
        /// s <see cref="SendTimeout"/> of 1000 milli seconds
        /// </summary>
        protected AbstractReplyProducingMessageHandler() {
            _channelTemplate = new MessageChannelTemplate();
            _channelTemplate.SendTimeout = DEFAULT_SEND_TIMEOUT;
        }


        /// <summary>
        /// get/set the output channel
        /// </summary>
        public IMessageChannel OutputChannel {
            get { return _outputChannel; }
            set { _outputChannel = value; }
        }

        /// <summary>
        /// Set the timeout for sending reply Messages.
        /// </summary>
        public TimeSpan SendTimeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        /// <summary>
        /// set a channel resolver to resolve the reply channel
        /// </summary>
        public IChannelResolver ChannelResolver {
            set {
                AssertUtils.ArgumentNotNull(value, "channelResolver must not be null");
                _channelResolver = value;
            }
        }

        /// <summary>
        /// set whether a reply is required
        /// </summary>
        public bool RequiresReply {
            set { _requiresReply = value; }
        }

        public IObjectFactory ObjectFactory {
            set {
                if(_channelResolver == null) {
                    _channelResolver = new ObjectFactoryChannelResolver(value);
                }
            }
        }

        protected override void HandleMessageInternal(IMessage message) {
            ReplyMessageHolder replyMessageHolder = new ReplyMessageHolder();
            HandleRequestMessage(message, replyMessageHolder);
            if(replyMessageHolder.IsEmpty) {
                if(_requiresReply) {
                    throw new MessageHandlingException(message, "handler '" + this + "' requires a reply, but no reply was received");
                }
                if(logger.IsDebugEnabled) {
                    logger.Debug("handler '" + this + "' produced no reply for request Message: " + message);
                }
                return;
            }
            IMessageChannel replyChannel = ResolveReplyChannel(message);
            MessageHeaders requestHeaders = message.Headers;
            foreach(MessageBuilder builder in replyMessageHolder.Builders) {
                builder.CopyHeadersIfAbsent(requestHeaders);
                SendReplyMessage(builder.Build(), replyChannel);
            }
        }

        protected abstract void HandleRequestMessage(IMessage requestMessage, ReplyMessageHolder replyMessageHolder);

        protected bool SendReplyMessage(IMessage replyMessage, IMessageChannel replyChannel) {
            if(logger.IsDebugEnabled) {
                logger.Debug("handler '" + this + "' sending reply Message: " + replyMessage);
            }
            return _channelTemplate.Send(replyMessage, replyChannel);
        }

        private IMessageChannel ResolveReplyChannel(IMessage requestMessage) {
            IMessageChannel replyChannel = OutputChannel;
            if(replyChannel == null) {
                object replyChannelHeader = requestMessage.Headers.ReplyChannel;
                if(replyChannelHeader != null) {
                    if(replyChannelHeader is IMessageChannel) {
                        replyChannel = (IMessageChannel)replyChannelHeader;
                    }
                    else if(replyChannelHeader is string) {
                        if(_channelResolver == null)
                            throw new InvalidOperationException("ChannelResolver is required for resolving a reply channel by name");

                        replyChannel = _channelResolver.ResolveChannelName((string)replyChannelHeader);
                    }
                    else {
                        throw new ChannelResolutionException("expected a IMessageChannel or string for 'replyChannel', but type is [" + replyChannelHeader.GetType() + "]");
                    }
                }
            }
            if(replyChannel == null) {
                throw new ChannelResolutionException("unable to resolve reply channel for message: " + requestMessage);
            }
            return replyChannel;
        }
    }
}
