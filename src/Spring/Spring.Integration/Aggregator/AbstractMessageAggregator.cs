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

using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Integration.Util;
using Spring.Util;
using MessageBuilder=Spring.Integration.Message.MessageBuilder;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// A base class for aggregating a group of Messages into a single Message.
    /// Extends {@link AbstractMessageBarrierHandler} and waits for a
    /// <em>complete</em> group of {@link Message Messages} to arrive. Subclasses
    /// must provide the implementation of the {@link #aggregateMessages(List)}
    /// method to combine the group of Messages into a single {@link Message}.
    /// 
    /// <p>
    /// The default strategy for determining whether a group is complete is based on
    /// the '<code>sequenceSize</code>' property of the header. Alternatively, a
    /// custom implementation of the {@link CompletionStrategy} may be provided.
    /// 
    /// <p>
    /// All considerations regarding <code>timeout</code> and grouping by
    /// <code>correlationId</code> from {@link AbstractMessageBarrierHandler} apply
    /// here as well.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class AbstractMessageAggregator : AbstractMessageBarrierHandler<IDictionary<object, IMessage>, object> {

        private volatile ICompletionStrategy _completionStrategy = new SequenceSizeCompletionStrategy();

	    /// <summary>
	    /// Strategy to determine whether the group of messages is complete.
	    /// </summary>
        public ICompletionStrategy CompletionStrategy  {
            set {
                AssertUtils.ArgumentNotNull(value, "'completionStrategy' must not be null");
                _completionStrategy = value;
            }
        }

        protected override MessageBarrier<IDictionary<object, IMessage>, object> CreateMessageBarrier() {
            return new MessageBarrier<IDictionary<object, IMessage>, object>(new Dictionary<object, IMessage>()); // TODO LinkedHashMap<object, IMessage>());
        }

        protected override void ProcessBarrier(MessageBarrier<IDictionary<object, IMessage>, object> barrier) {
            IList<IMessage> messageList = new List<IMessage>(barrier.Messages.Values);
            if (!barrier.IsComplete && messageList.Count > 0) {
                if (_completionStrategy.IsComplete(messageList)) {
                    barrier.SetComplete();
                }
            }
            if (barrier.IsComplete) {
                RemoveBarrier(barrier.CorrelationId);
                IMessage result = AggregateMessages(messageList);
                if (result != null) {
                    if (result.Headers.CorrelationId == null) {
                        result = MessageBuilder.FromMessage(result).SetCorrelationId(barrier.CorrelationId).Build();
                    }
                    SendReply(result, ResolveReplyChannelFromMessage(messageList[0]));
                }
            }
        }
	
        protected override bool CanAddMessage(IMessage message, MessageBarrier<IDictionary<object, IMessage>, object> barrier) {
            if (!base.CanAddMessage(message, barrier)) {
                return false;
            }
            if (barrier.Messages.ContainsKey(message.Headers.Id)) {
                logger.Debug("The barrier has received message: " + message + ", but it already contains a similar message: " + barrier.Messages[message.Headers.Id]);
                return false;
            }
            return true;
        }
	
        protected override void DoAddMessage(IMessage message, MessageBarrier<IDictionary<object, IMessage>, object> barrier) {
            DictionaryUtils.Put(barrier.Messages, message.Headers.Id, message);
        }

        public abstract IMessage AggregateMessages(IList<IMessage> messages);
    }
}