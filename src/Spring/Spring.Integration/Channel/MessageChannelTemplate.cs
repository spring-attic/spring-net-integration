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
using System.Data;
using System.Threading;
using Common.Logging;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Selector;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Integration.Channel {

    /// <summary>
    /// This is the central class for invoking message exchange operations across
    /// <see cref="IMessageChannel"/>s. It supports one-way send and receive calls as well
    /// as request/reply.
    /// <p>
    /// To enable transactions, configure the 'transactionManager' property with a
    /// reference to an instance of Spring's <see cref="IPlatformTransactionManager"/> 
    /// strategy and optionally provide the other transactional attributes
    /// (e.g. 'propagationBehaviorName').
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageChannelTemplate : IInitializingObject {

        protected ILog _logger = LogManager.GetLogger(typeof (MessageChannelTemplate));

        private volatile IMessageChannel _defaultChannel;

        private /*volatile*/ TimeSpan _sendTimeout = TimeSpan.FromMilliseconds(-1);

        private /*volatile*/ TimeSpan _receiveTimeout = TimeSpan.FromMilliseconds(-1);

        private volatile IPlatformTransactionManager _transactionManager;

        private volatile TransactionTemplate _transactionTemplate;

        private volatile TransactionPropagation _propagationBehavior = TransactionPropagation.Required;

        private volatile IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

        private /*volatile*/ TimeSpan _transactionTimeout = TimeSpan.FromMilliseconds(-1);

        private volatile bool _readOnly;

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();


        /// <summary>
        /// Create a MessageChannelTemplate with no default channel. Note, that one
        /// may be provided by invoking <see cref="DefaultChannel"/>.
        /// </summary>
        public MessageChannelTemplate() {
        }

        /// <summary>
        /// Create a MessageChannelTemplate with the given default channel.
        /// </summary>
        public MessageChannelTemplate(IMessageChannel defaultChannel) {
            _defaultChannel = defaultChannel;
        }

        /// <summary>
        /// set the default MessageChannel to use when invoking the send and/or
        /// receive methods that do not expect a channel parameter.
        /// </summary>
        public IMessageChannel DefaultChannel {
            set { _defaultChannel = value; }
        }

        /// <summary>
        /// set the timeout value to use for send operations.
        /// </summary>
        public TimeSpan SendTimeout {
            set {
                lock(this) {
                    _sendTimeout = value;
                }
            }
        }

        /// <summary>
        /// set the timeout value to use for receive operations.
        /// </summary>
        public TimeSpan ReceiveTimeout {
            set {
                lock(this) {
                    _receiveTimeout = value;
                }
            }
        }

        /// <summary>
        /// Specify a transaction manager to use for all exchange operations.
        /// If none is provided, then the operations will occur without any
        /// transactional behavior (i.e. there is no default transaction manager).
        /// </summary>
        public IPlatformTransactionManager TransactionManager {
            set { _transactionManager = value; }
        }

        /// <summary>
        /// set the propagation behavior
        /// </summary>
        public TransactionPropagation PropagationBehavior {
            set { _propagationBehavior = value; }
        }

        /// <summary>
        /// set the isolation level
        /// </summary>
        public IsolationLevel IsolationLevel {
            set { _isolationLevel = value; }
        }

        /// <summary>
        /// set the transaction timeout
        /// </summary>
        public TimeSpan TransactionTimeout {
            set {
                lock(this) {
                    _transactionTimeout = value;
                }
            }
        }

        /// <summary>
        /// set whether the transaction is readonly
        /// </summary>
        public bool TransactionReadOnly {
            set { _readOnly = value; }
        }

        private TransactionTemplate TransactionTemplate {
            get {
                if (!_initialized) {
                    AfterPropertiesSet();
                }
                return _transactionTemplate;
            }
        }

        /// <summary>
        /// called by the framework after all properties are set
        /// </summary>
        public void AfterPropertiesSet() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }
                if(_transactionManager != null) {
                    TransactionTemplate template = new TransactionTemplate(_transactionManager);
                    template.PropagationBehavior = _propagationBehavior;
                    template.TransactionIsolationLevel = _isolationLevel;
                    // TODO check TransactionTemplate.TransactionTimeout
                    template.TransactionTimeout = (int)_transactionTimeout.TotalMilliseconds;
                    template.ReadOnly = _readOnly;
                    _transactionTemplate = template;
                }
                _initialized = true;
            }
        }

        /// <summary>
        /// send <paramref name="message"/> to the <see cref="RequiredDefaultChannel"/>
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns><c>true</c>if the mesage was send <c>false</c> otherwise</returns>
        public bool Send(IMessage message) {
            return Send(message, RequiredDefaultChannel);
        }

        /// <summary>
        /// send <paramref name="message"/> to channel <paramref name="channel"/>
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="channel">the channel <paramref name="message"> should be send through"/></param>
        /// <returns><c>true</c>if the mesage was send <c>false</c> otherwise</returns>
        public bool Send(IMessage message, IMessageChannel channel) {
            TransactionTemplate txTemplate = TransactionTemplate;
            if (txTemplate != null) {
                return (bool)txTemplate.Execute(delegate { return DoSend(message, channel); });
            }
            return DoSend(message, channel);
        }

        /// <summary>
        /// receive a <see cref="IMessage"/> from the <see cref="RequiredDefaultChannel"/>
        /// </summary>
        /// <returns>the received <see cref="IMessage"/> or <c>null</c> if no message could be received</returns>
        public IMessage Receive() {
            IPollableChannel channel = RequiredDefaultChannel as IPollableChannel;
            
            AssertUtils.State(channel != null, "The 'defaultChannel' must be a PollableChannel for receive operations.");
            return Receive(channel);
        }

        /// <summary>
        /// receive a <see cref="IMessage"/> from <paramref name="channel"/>
        /// </summary>
        /// <param name="channel">the channel the <see cref="IMessage"/> should be received from</param>
        /// <returns>the received <see cref="IMessage"/> or <c>null</c> if no message could be received</returns>
        public IMessage Receive(IPollableChannel channel) {
            TransactionTemplate txTemplate = TransactionTemplate;
            if (txTemplate != null) {
                return (IMessage) txTemplate.Execute(delegate { return DoReceive(channel); });
            }
            return DoReceive(channel);
        }

        /// <summary>
        /// send <paramref name="request"/> to <see cref="RequiredDefaultChannel"/> and receive reply
        /// </summary>
        /// <param name="request">the request to send</param>
        /// <returns>the received <see cref="IMessage"/> or <c>null</c> if no message could be received</returns>
        public IMessage SendAndReceive(IMessage request) {
            return SendAndReceive(request, RequiredDefaultChannel);
        }

        /// <summary>
        /// send <paramref name="request"/> to <paramref name="channel"/> and receive reply
        /// </summary>
        /// <param name="request">the request to send</param>
        /// <param name="channel">the channel to send to and receive from</param>
        /// <returns>the received <see cref="IMessage"/> or <c>null</c> if no message could be received</returns>
        public IMessage SendAndReceive(IMessage request, IMessageChannel channel) {
            TransactionTemplate txTemplate = TransactionTemplate;
            if (txTemplate != null) {
                return (IMessage) txTemplate.Execute(delegate { return DoSendAndReceive(request, channel); });
            }
            return DoSendAndReceive(request, channel);
        }

        private bool DoSend(IMessage message, IMessageChannel channel) {
            AssertUtils.ArgumentNotNull(channel, "channel", "channel must not be null");
            TimeSpan timeout = _sendTimeout;
            bool sent = (timeout.TotalMilliseconds >= 0) ? channel.Send(message, timeout) : channel.Send(message);
            if (!sent && _logger.IsTraceEnabled) {
                _logger.Trace("failed to send message to channel '" + channel + "' within timeout: " + timeout);
            }
            return sent;
        }

        private IMessage DoReceive(IPollableChannel channel) {
            AssertUtils.ArgumentNotNull(channel, "channel", "channel must not be null");
            TimeSpan timeout = _receiveTimeout;
            IMessage message = (timeout.TotalMilliseconds >= 0) ? channel.Receive(timeout) : channel.Receive();
            if (message == null && _logger.IsTraceEnabled) {
                _logger.Trace("failed to receive message from channel '" + channel + "' within timeout: " + timeout);
            }
            return message;
        }

        private IMessage DoSendAndReceive(IMessage request, IMessageChannel channel) {
            TemporaryReplyChannel replyChannel = new TemporaryReplyChannel(_receiveTimeout);
            request = MessageBuilder.FromMessage(request)
                    .SetReplyChannel(replyChannel)
                    .SetErrorChannel(replyChannel)
                    .Build();
            if (!DoSend(request, channel)) {
                return null;
            }
            return DoReceive(replyChannel);
        }

        private IMessageChannel RequiredDefaultChannel {
            get {
                AssertUtils.State(_defaultChannel != null, "No 'defaultChannel' specified for MessageChannelTemplate. Unable to invoke methods without a channel argument.");
                return _defaultChannel;
            }
        }

        private class TemporaryReplyChannel : IPollableChannel {
            private volatile IMessage _message;

            private readonly TimeSpan _receiveTimeout;

            private readonly CountDownLatch _latch = new CountDownLatch(1);


            public TemporaryReplyChannel(TimeSpan receiveTimeout) {
                _receiveTimeout = receiveTimeout;
            }

            public string Name {
                get { return "temporaryReplyChannel"; }
            }

            public IMessage Receive() {
                return Receive(TimeSpan.FromMilliseconds(-1));
            }

            public IMessage Receive(TimeSpan timeout) {
                try {
                    if(_receiveTimeout.TotalMilliseconds < 0) {
                        long count = _latch.Count;
                        _latch.Await();
                    }
                    else {
                        int moin = (int)_receiveTimeout.TotalMilliseconds;
                        _latch.Await(_receiveTimeout);
                    }
                }
                catch(ThreadInterruptedException) {
                    Thread.CurrentThread.Interrupt();
                }
                return _message;
            }

            public bool Send(IMessage message) {
                return Send(message, TimeSpan.FromMilliseconds(-1));
            }

            public bool Send(IMessage message, TimeSpan timeout) {
                _message = message;
                _latch.CountDown();
                return true;
            }

            public IList<IMessage> Clear() {
                return null;
            }

            public IList<IMessage> Purge(IMessageSelector selector) {
                return null;
            }
        }
    }
}
