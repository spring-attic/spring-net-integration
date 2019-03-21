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
using Common.Logging;
using Spring.Integration.Channel;
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Scheduling;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;
using Spring.Util;
using MessageBuilder = Spring.Integration.Message.MessageBuilder;
using MessageHandlingException = Spring.Integration.Message.MessageHandlingException;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// Base class for {@link MessageBarrier}-based Message Handlers. A
    /// {@link MessageHandler} implementation that waits for a group of
    /// {@link Message Messages} to arrive and processes them together. Uses a
    /// {@link MessageBarrier} to store messages and to decide how the messages
    /// should be released.
    /// <p>
    /// Each {@link Message} that is received by this handler will be associated
    /// with a group based upon the '<code>correlationId</code>' property of its
    /// header. If no such property is available, a {@link MessageHandlingException}
    /// will be thrown.
    /// <p>
    /// The '<code>timeout</code>' value determines how long to wait for the complete
    /// group after the arrival of the first {@link Message} of the group. The
    /// default value is 1 minute. If the timeout elapses prior to completion, then
    /// Messages with that timed-out 'correlationId' will be sent to the
    /// 'discardChannel' if provided unless 'sendPartialResultsOnTimeout' is set to
    /// true in which case the incomplete group will be sent to the output channel.
    /// <p>
    /// Subclasses must decide what kind of a Map they want to use, what is the logic
    /// for adding messages to the barrier through the '<code>doAddMessage</code>'
    /// method.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractMessageBarrierHandler<T, K> : AbstractMessageHandler, IObjectFactoryAware, IInitializingObject where T : IDictionary<K, IMessage> {

        public static TimeSpan DEFAULT_SEND_TIMEOUT = TimeSpan.FromMilliseconds(1000);

        public static TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMilliseconds(60000);

        public static TimeSpan DEFAULT_REAPER_INTERVAL = TimeSpan.FromMilliseconds(1000);

        public static int DEFAULT_TRACKED_CORRRELATION_ID_CAPACITY = 1000;

        protected ILog logger = LogManager.GetLogger(typeof(AbstractMessageBarrierHandler<T, K>));

        private volatile IMessageChannel _outputChannel;

        private MessageChannelTemplate _channelTemplate = new MessageChannelTemplate();

        private volatile IMessageChannel _discardChannel;

        // TODO protected final ConcurrentMap<Object, MessageBarrier<T,K>> barriers = new ConcurrentHashMap<Object, MessageBarrier<T,K>>();
        protected IDictionary<object, MessageBarrier<T, K>> _barriers = new Dictionary<object, MessageBarrier<T, K>>();

        private /*volatile*/ TimeSpan _timeout = DEFAULT_TIMEOUT;

        private volatile bool _sendPartialResultOnTimeout = false;

        private /*volatile*/ TimeSpan _reaperInterval = DEFAULT_REAPER_INTERVAL;

        private volatile int _trackedCorrelationIdCapacity = DEFAULT_TRACKED_CORRRELATION_ID_CAPACITY;

        protected volatile IBlockingQueue<object> _trackedCorrelationIds;

        private volatile bool _autoStartup = true;

        private volatile ITaskScheduler _taskScheduler;

        private volatile IScheduledFuture<object> _reaperFutureTask;

        private volatile bool _initialized;

        private readonly object _lifecycleMonitor = new object();


        /// <summary>
        /// default ctor
        /// </summary>
        protected AbstractMessageBarrierHandler() {
            _channelTemplate.SendTimeout = DEFAULT_SEND_TIMEOUT;
        }

        /// <summary>
        /// set the output channel
        /// </summary>
        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        /// <summary>
        /// Specify a channel for sending Messages that arrive after their
        /// aggregation group has either completed or timed-out.
        /// </summary>
        public IMessageChannel DiscardChannel {
            set { _discardChannel = value; }
        }

        /// <summary>
        /// Specify whether to aggregate and send the resulting Message when the
        /// timeout elapses prior to the CompletionStrategy returning true.
        /// </summary>
        public bool SendPartialResultOnTimeout {
            set { _sendPartialResultOnTimeout = value; }
        }

        /// <summary>
        /// Set the interval in milliseconds for the reaper thread. Default is 1000.
        /// </summary>
        public TimeSpan ReaperInterval {
            set {
                AssertUtils.IsTrue(value.TotalMilliseconds > 0, "'reaperInterval' must be a positive value");
                lock(this) {
                    _reaperInterval = value;
                }
            }
        }

        /// <summary>
        /// Set the number of completed correlationIds to track. Default is 1000.
        /// </summary>
        public int TrackedCorrelationIdCapacity {
            set {
                AssertUtils.IsTrue(value > 0, "'trackedCorrelationIdCapacity' must be a positive value");
                _trackedCorrelationIdCapacity = value;
            }
        }

        /// <summary>
        /// Maximum time to wait (in milliseconds) for the completion strategy to
        /// become true. The default is 60000 (1 minute).
        /// </summary>
        public TimeSpan Timeout {
            set {
                AssertUtils.IsTrue(value.TotalMilliseconds >= 0, "'timeout' must not be negative");
                lock(this) {
                    _timeout = value;
                }
            }
        }

        public TimeSpan SendTimeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        public ITaskScheduler TaskScheduler {
            set {
                AssertUtils.ArgumentNotNull(value, "taskScheduler must not be null");
                _taskScheduler = value;
            }
        }

        public bool AutoStartup {
            set { _autoStartup = value; }
        }

        public IObjectFactory ObjectFactory {
            set {
                if(_taskScheduler == null) {
                    _taskScheduler = IntegrationContextUtils.GetRequiredTaskScheduler(value);
                }
            }
        }

        public void AfterPropertiesSet() {
            lock(_lifecycleMonitor) {
                if(!_initialized) {
                    _trackedCorrelationIds = new ArrayBlockingQueue<object>(_trackedCorrelationIdCapacity);
                    if(_autoStartup) {
                        Start();
                    }
                    _initialized = true;
                }
            }
        }

        public bool IsRunning() {
            lock(_lifecycleMonitor) {
                return _reaperFutureTask != null;
            }
        }

        public void Start() {
            lock(_lifecycleMonitor) {
                if(IsRunning()) {
                    return;
                }
                AssertUtils.State(_taskScheduler != null, "TaskScheduler must not be null");
                _reaperFutureTask = _taskScheduler.Schedule(new PrunerTask(this), new IntervalTrigger(_reaperInterval));
            }
        }

        public void Stop() {
            lock(_lifecycleMonitor) {
                if(IsRunning()) {
                    _reaperFutureTask.Cancel(true);
                }
            }
        }

        protected override void HandleMessageInternal(IMessage message) {
            if(!_initialized) {
                AfterPropertiesSet();
            }
            object correlationId = message.Headers.CorrelationId;
            if(correlationId == null) {
                throw new MessageHandlingException(message, GetType().Name + " requires the 'correlationId' property");
            }
            if(_trackedCorrelationIds.Contains(correlationId)) {
                if(logger.IsDebugEnabled) {
                    logger.Debug("Handling of Message group with correlationId '" + correlationId + "' has already completed or timed out.");
                }
                DiscardMessage(message);
            }
            else {
                ProcessMessage(message, correlationId);
            }
        }

        private void DiscardMessage(IMessage message) {
            if(_discardChannel != null) {
                bool sent = _channelTemplate.Send(message, _discardChannel);
                if(!sent && logger.IsWarnEnabled) {
                    logger.Warn("unable to send to 'discardChannel', message: " + message);
                }
            }
        }

        private void ProcessMessage(IMessage message, object correlationId) {
            //MessageBarrier<T,K> barrier = _barriers.putIfAbsent(correlationId, createMessageBarrier());
            MessageBarrier<T, K> barrier;

            if(_barriers.ContainsKey(message.Headers.CorrelationId)) {
                barrier = _barriers[message.Headers.CorrelationId];
            }
            else {
                 barrier = CreateMessageBarrier();
                 _barriers.Add(correlationId, barrier);
            }
            lock(barrier) {
                if(CanAddMessage(message, barrier)) {
                    DoAddMessage(message, barrier);
                }
                ProcessBarrier(barrier);
            }
        }

        protected void SendReplies(ICollection<IMessage> messages, IMessageChannel defaultReplyChannel) {
            if(messages.Count == 0) {
                return;
            }
            foreach(IMessage result in messages) {
                SendReply(result, defaultReplyChannel);
            }
        }

        protected void SendReply(IMessage message, IMessageChannel defaultReplyChannel) {
            IMessageChannel replyChannel = _outputChannel;
            if(replyChannel == null) {
                replyChannel = ResolveReplyChannelFromMessage(message);
                if(replyChannel == null) {
                    replyChannel = defaultReplyChannel;
                }
            }
            if(replyChannel != null) {
                if(defaultReplyChannel != null && !defaultReplyChannel.Equals(replyChannel)) {
                    message = MessageBuilder.FromMessage(message).SetHeaderIfAbsent(MessageHeaders.REPLY_CHANNEL, defaultReplyChannel).Build();
                }
                _channelTemplate.Send(message, replyChannel);
            }
            else if(logger.IsWarnEnabled) {
                logger.Warn("unable to determine reply target for aggregation result: " + message);
            }
        }

        protected IMessageChannel ResolveReplyChannelFromMessage(IMessage message) {
            object replyChannel = message.Headers.ReplyChannel;
            if(replyChannel != null) {
                if(replyChannel is IMessageChannel) {
                    return (IMessageChannel)replyChannel;
                }
                if(logger.IsWarnEnabled) {
                    logger.Warn("Aggregator can only reply to a 'replyChannel' of type MessageChannel.");
                }
            }
            return null;
        }

        protected void RemoveBarrier(object correlationId) {
            if(_barriers.Remove(correlationId)) {
                lock(_trackedCorrelationIds) {
                    bool added = _trackedCorrelationIds.Offer(correlationId);
                    if(!added) {
                        object dummy;
                        _trackedCorrelationIds.Poll(out dummy);
                        _trackedCorrelationIds.Offer(correlationId);
                    }
                }
            }
        }

        /// <summary>
        /// Verifies that a message can be added to the barrier. To be overridden by subclasses, which may add 
        /// their own verifications. Subclasses overriding this method must call the method from the superclass.
        /// </summary>
        protected virtual bool CanAddMessage(IMessage message, MessageBarrier<T, K> barrier) {
            if(barrier.IsComplete) {
                if(logger.IsDebugEnabled) {
                    logger.Debug("Message received after aggregation has already completed: " + message);
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Factory method for creating a MessageBarrier implementation.
        /// </summary>
        protected abstract MessageBarrier<T, K> CreateMessageBarrier();

        /// <summary>
        /// A method for processing the information in the message barrier after a message has been added or on pruning.
        /// The decision as to whether the messages from the {@link MessageBarrier}
        /// can be released normally belongs here, although calling code may forcibly set the MessageBarrier's 'complete'
        /// flag to true before invoking the method.
        /// </summary>
        /// <param name="barrier">the {@link MessageBarrier} to be processed</param>
        protected abstract void ProcessBarrier(MessageBarrier<T, K> barrier);

        /// <summary>
        /// A method implemented by subclasses to add the incoming message to the message barrier. This is deferred to subclasses,
        /// as they should have full control over how the messages are indexed in the MessageBarrier.
        /// </summary>
        protected abstract void DoAddMessage(IMessage message, MessageBarrier<T, K> barrier);

        /// <summary>
        /// A task that runs periodically, pruning the timed-out message barriers.
        /// </summary>
        private class PrunerTask : IRunnable {
            private readonly AbstractMessageBarrierHandler<T, K> _outer;

            public PrunerTask(AbstractMessageBarrierHandler<T, K> outer) {
                _outer = outer;
            }
            public void Run() {
                DateTime currentTime = DateTime.Now;
                foreach(KeyValuePair<object, MessageBarrier<T, K>> entry in _outer._barriers) {
                    if(currentTime - entry.Value.Timestamp >= _outer._timeout) {
                        MessageBarrier<T, K> barrier = entry.Value;
                        lock(barrier) {
                            _outer.RemoveBarrier(entry.Key);
                            if(_outer._sendPartialResultOnTimeout) {
                                barrier.SetComplete();
                                _outer.ProcessBarrier(barrier);
                            }
                            else {
                                foreach(IMessage message in barrier.Messages.Values) {
                                    if(_outer.logger.IsDebugEnabled) {
                                        _outer.logger.Debug("Handling of Message group with correlationId '" + entry.Key + "' has timed out.");
                                    }
                                    _outer.DiscardMessage(message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}