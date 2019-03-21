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
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Scheduling;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Gateway {

    /// <summary>
    /// A convenient base class for connecting application code to
    /// {@link MessageChannel}s for sending, receiving, or request-reply operations.
    /// Exposes setters for configuring request and reply {@link MessageChannel}s as
    /// well as the timeout values for sending and receiving Messages.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractMessagingGateway : AbstractEndpoint, IMessagingGateway {

        private volatile IMessageChannel _requestChannel;

        private volatile IMessageChannel _replyChannel;

        private readonly MessageChannelTemplate _channelTemplate = new MessageChannelTemplate();

        private volatile bool _shouldThrowErrors = true;

        private volatile bool _initialized;

        private volatile AbstractEndpoint _replyMessageCorrelator;

        private readonly object _replyMessageCorrelatorMonitor = new object();

        /// <summary>
        /// Set the request channel to which request messages will be sent
        /// </summary>
        public IMessageChannel RequestChannel {
            set { _requestChannel = value; }
        }

        /// <summary>
        /// Set the reply channel from which reply messages will be received. If no reply channel is provided, 
        /// this template will always use an anonymous, temporary channel for handling replies.
        /// </summary>
        public IMessageChannel ReplyChannel {
            set { _replyChannel = value; }
        }

        /// <summary>
        /// Set the timeout value for sending request messages. If not
        /// explicitly configured, the default is an indefinite timeout.
        /// </summary>
        public TimeSpan RequestTimeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        /// <summary>
        /// Set the timeout value for receiving reply messages. If not
        /// explicitly configured, the default is an indefinite timeout.
        /// </summary>
        public TimeSpan ReplyTimeout {
            set { _channelTemplate.ReceiveTimeout = value; }
        }

        /**
         * Specify whether the Throwable payload of a received {@link ErrorMessage}
         * should be extracted and thrown from a send-and-receive operation.
         * Otherwise, the ErrorMessage would be returned just like any other
         * reply Message. The default is <code>true</code>.
         */
        public bool ShouldThrowErrors {
            set { _shouldThrowErrors = value; }
        }

        protected override void OnInit() {
            _initialized = true;
        }

        private void InitializeIfNecessary() {
            if(!_initialized) {
                AfterPropertiesSet();
            }
        }

        public void Send(object obj) {
            InitializeIfNecessary();
            AssertUtils.State(_requestChannel != null, "send is not supported, because no request channel has been configured");
            IMessage message = ToMessage(obj);
            AssertUtils.ArgumentNotNull(message, "message");
            if(!_channelTemplate.Send(message, _requestChannel)) {
                throw new MessageDeliveryException(message, "failed to send Message to channel");
            }
        }

        public object Receive() {
            InitializeIfNecessary();
            AssertUtils.State(_replyChannel != null && (_replyChannel is IPollableChannel),
                    "receive is not supported, because no pollable reply channel has been configured");
            IMessage message = _channelTemplate.Receive((IPollableChannel)_replyChannel);
            return FromMessage(message);
        }

        public object SendAndReceive(object obj) {
            return SendAndReceive(obj, true);
        }

        public IMessage SendAndReceiveMessage(object obj) {
            return (IMessage)SendAndReceive(obj, false);
        }

        private object SendAndReceive(object obj, bool shouldMapMessage) {
            IMessage request = ToMessage(obj);
            IMessage reply = SendAndReceiveMessage(request);
            if(!shouldMapMessage) {
                return reply;
            }
            return FromMessage(reply);
        }

        private IMessage SendAndReceiveMessage(IMessage message) {
            InitializeIfNecessary();
            AssertUtils.ArgumentNotNull(message, "request message must not be null");
            if(_requestChannel == null) {
                throw new MessageDeliveryException(message, "No request channel available. Cannot send request message.");
            }
            if(_replyChannel != null && _replyMessageCorrelator == null) {
                RegisterReplyMessageCorrelator();
            }
            IMessage reply = _channelTemplate.SendAndReceive(message, _requestChannel);
            if(reply != null && _shouldThrowErrors && reply is ErrorMessage) {
                Exception error = (Exception)((ErrorMessage)reply).Payload;
                if(error is SystemException) {
                    throw error;
                }
                throw new MessagingException("gateway received checked Exception", error);
            }
            return reply;
        }

        private void RegisterReplyMessageCorrelator() {
            lock(_replyMessageCorrelatorMonitor) {
                if(_replyMessageCorrelator != null) {
                    return;
                }
                AbstractEndpoint correlator = null;
                IMessageHandler handler = new LocalReplyProducingMessageHandler();

                if(_replyChannel is ISubscribableChannel) {
                    correlator = new EventDrivenConsumer((ISubscribableChannel)_replyChannel, handler);
                }
                else if(_replyChannel is IPollableChannel) {
                    PollingConsumer endpoint = new PollingConsumer(
                            (IPollableChannel)_replyChannel, handler);
                    endpoint.Trigger = new IntervalTrigger(TimeSpan.FromMilliseconds(10));
                    endpoint.ObjectFactory = ObjectFactory;
                    endpoint.AfterPropertiesSet();
                    correlator = endpoint;
                }
                if(IsRunning) {
                    if(correlator == null)
                        throw new InvalidOperationException("correlator must not be null");
                    
                    ((ILifecycle)correlator).Start();
                }
                _replyMessageCorrelator = correlator;
            }
        }

        // guarded by super#lifecycleLock
        protected override void DoStart() {
            if(_replyMessageCorrelator != null) {
                ((ILifecycle)_replyMessageCorrelator).Start();
            }
        }

        // guarded by super#lifecycleLock
        protected override void DoStop() {
            if(_replyMessageCorrelator != null) {
                ((ILifecycle)_replyMessageCorrelator).Stop();
            }
        }

        /**
         * Subclasses must implement this to map from an Object to a Message.
         */
        protected abstract IMessage ToMessage(object obj);

        /**
         * Subclasses must implement this to map from a Message to an Object.
         */
        protected abstract object FromMessage(IMessage message);

        private class LocalReplyProducingMessageHandler : AbstractReplyProducingMessageHandler {

            protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
                replyHolder.Set(message);
            }
        };
    }
}
