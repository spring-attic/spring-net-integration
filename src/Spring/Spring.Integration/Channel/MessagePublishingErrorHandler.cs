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
using Common.Logging;
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Channel {

    /// <summary>
    /// {@link ErrorHandler} implementation that sends an {@link ErrorMessage} to a
    /// {@link MessageChannel}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Iwein Fuld</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class MessagePublishingErrorHandler : IErrorHandler, IObjectFactoryAware {

        private readonly ILog logger = LogManager.GetLogger(typeof(MessagePublishingErrorHandler));

        private volatile IChannelResolver _channelResolver;

        private volatile IMessageChannel _defaultErrorChannel;

        private /*volatile*/ TimeSpan _sendTimeout = new TimeSpan(0, 0, 0, 0, 1000);


        /// <summary>
        /// create a new <see cref="MessagePublishingErrorHandler"/> with a channel resolver configured
        /// in the object factory
        /// </summary>
        public MessagePublishingErrorHandler() {
        }

        /// <summary>
        /// create a new <see cref="MessagePublishingErrorHandler"/> with the <paramref name="channelResolver"/>
        /// </summary>
        /// <param name="channelResolver"></param>
        public MessagePublishingErrorHandler(IChannelResolver channelResolver) {
            AssertUtils.ArgumentNotNull(channelResolver, "channelResolver", "channelResolver must not be null");
            _channelResolver = channelResolver;
        }

        /// <summary>
        /// set the default error channel
        /// </summary>
        public IMessageChannel DefaultErrorChannel {
            set { _defaultErrorChannel = value; }
        }

        /// <summary>
        /// set the timeout for sending the erros message
        /// </summary>
        public TimeSpan SendTimeout {
            set {
                lock(this) {
                    _sendTimeout = value;
                }
            }
        }

        #region IObjectFactoryAware Members

        public IObjectFactory ObjectFactory {
            set {
                AssertUtils.ArgumentNotNull(value, "ObjectFactory", "ObjectFactory must not be null");
                if(_channelResolver == null) {
                    _channelResolver = new ObjectFactoryChannelResolver(value);
                }

            }
        }

        #endregion

        #region IErrorHandler Members

        public void HandleError(Exception ex) {
            MessagingException messagingException = ex as MessagingException;
            IMessage failedMessage = messagingException == null ? null : messagingException.FailedMessage;

            IMessageChannel errorChannel = ResolveErrorChannel(failedMessage);
            bool sent = false;
            if(errorChannel != null) {
                try {
                    if(_sendTimeout.TotalMilliseconds >= 0) {
                        sent = errorChannel.Send(new ErrorMessage(ex), _sendTimeout);
                    }
                    else {
                        sent = errorChannel.Send(new ErrorMessage(ex));
                    }
                }
                catch(Exception errorDeliveryError) { // message will be logged only
                    if(logger.IsWarnEnabled) {
                        logger.Warn("Error message was not delivered.", errorDeliveryError);
                    }
                }
            }
            if(!sent && logger.IsErrorEnabled) {
                if(failedMessage != null) {
                    logger.Error("failure occurred in messaging task with message: " + failedMessage, ex);
                }
                else {
                    logger.Error("failure occurred in messaging task", ex);
                }
            }
        }
        #endregion

        private IMessageChannel ResolveErrorChannel(IMessage failedMessage) {
            if(_defaultErrorChannel == null && _channelResolver != null) {
                _defaultErrorChannel = _channelResolver.ResolveChannelName(IntegrationContextUtils.ErrorChannelObjectName);
            }
            
            if(failedMessage == null || failedMessage.Headers.ErrorChannel == null) {
                return _defaultErrorChannel;
            }
            
            IMessageChannel errorChannel = failedMessage.Headers.ErrorChannel as IMessageChannel;
            if(errorChannel != null) {
                return errorChannel;
            }
            
            string errorChannelName = failedMessage.Headers.ErrorChannel as string;
            if (errorChannelName == null)
                throw new ArgumentException("Unsupported error channel header type. Expected IMessageChannel or string");

            if(_channelResolver == null)
                throw new ApplicationException("channelResolver must not be null");

            return _channelResolver.ResolveChannelName(errorChannelName);
        }

    }
}