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
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// An {@link AbstractMessageBarrierHandler} that waits for a group of
    /// {@link Message Messages} to arrive and re-sends them in order, sorted
    /// by their <code>sequenceNumber</code>.
    /// <p>
    /// This handler can either release partial sequences of messages or can
    /// wait for the whole sequence to arrive before re-sending them.
    /// <p>
    /// All considerations regarding <code>timeout</code> and grouping by
    /// '<code>correlationId</code>' from {@link AbstractMessageBarrierHandler}
    /// apply here as well.
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class Resequencer : AbstractMessageBarrierHandler<SortedDictionary<int, IMessage>, int> {

        private volatile bool _releasePartialSequences = true;


        public bool ReleasePartialSequences {
            set { _releasePartialSequences = value; }
        }

        protected override MessageBarrier<SortedDictionary<int, IMessage>, int> CreateMessageBarrier() {
            MessageBarrier<SortedDictionary<int, IMessage>, int> messageBarrier
                = new MessageBarrier<SortedDictionary<int, IMessage>, int>(new SortedDictionary<int, IMessage>()); // TODO (new TreeMap<Integer, Message<?>>());
            messageBarrier.Messages.Add(0, CreateFlagMessage(0));
            return messageBarrier;
        }

        protected override void ProcessBarrier(MessageBarrier<SortedDictionary<int, IMessage>, int> barrier) {
            if(HasReceivedAllMessages(barrier.Messages)) {
                barrier.SetComplete();
            }
            IList<IMessage> releasedMessages = ReleaseAvailableMessages(barrier);
            if(releasedMessages != null && releasedMessages.Count > 0) {
                IMessage lastMessage = releasedMessages[releasedMessages.Count - 1];
                if(lastMessage.Headers.SequenceNumber.Equals(lastMessage.Headers.SequenceSize - 1)) {
                    RemoveBarrier(barrier.CorrelationId);
                }
                SendReplies(releasedMessages, ResolveReplyChannelFromMessage(releasedMessages[0]));
            }
        }

        private static bool HasReceivedAllMessages(IDictionary<int, IMessage> messages) {
            IMessage firstMessage = messages[GetFirstKey(messages)];
            IMessage lastMessage = messages[GetLastKey(messages)];
            return (lastMessage.Headers.SequenceNumber.Equals(lastMessage.Headers.SequenceSize)
                    && (lastMessage.Headers.SequenceNumber - firstMessage.Headers.SequenceNumber == messages.Count - 1));
        }

        private IList<IMessage> ReleaseAvailableMessages(MessageBarrier<SortedDictionary<int, IMessage>, int> barrier) {
            if(_releasePartialSequences || barrier.IsComplete) {
                IList<IMessage> releasedMessages = new List<IMessage>();

                IMessage flagMessage = null;
                int lastReleasedSequenceNumber = 0;
                foreach(IMessage msg in barrier.Messages.Values) {
                    if(flagMessage == null) {
                        flagMessage = msg;
                        lastReleasedSequenceNumber = flagMessage.Headers.SequenceNumber;
                        continue;
                    }

                    if(lastReleasedSequenceNumber == msg.Headers.SequenceNumber - 1) {
                        releasedMessages.Add(msg);
                        lastReleasedSequenceNumber = msg.Headers.SequenceNumber;
                    }
                    else {
                        break;
                    }
                }

                if(flagMessage != null) {
                    // remove the flagMessage and all released messages form the barrier
                    barrier.Messages.Remove(flagMessage.Headers.SequenceNumber);
                    foreach (IMessage msg in releasedMessages) {
                        barrier.Messages.Remove(msg.Headers.SequenceNumber);
                    }

                    //re-insert the flag so that we know where to start releasing next
                    barrier.Messages.Add(lastReleasedSequenceNumber, CreateFlagMessage(lastReleasedSequenceNumber));
                    return releasedMessages;
                }
            }
            return new List<IMessage>();
        }

        protected override bool CanAddMessage(IMessage message, MessageBarrier<SortedDictionary<int, IMessage>, int> barrier) {
            if (!base.CanAddMessage(message, barrier)) {
                return false;
            }
            IMessage flagMessage = barrier.Messages[GetFirstKey(barrier.Messages)];
            if (barrier.Messages.ContainsKey(message.Headers.SequenceNumber) || flagMessage.Headers.SequenceNumber >= message.Headers.SequenceNumber) {
                logger.Debug("A message with the same sequence number has been already received: " + message);
                return false;
            }
            IMessage lastMessage = barrier.Messages[GetLastKey(barrier.Messages)];
            if (lastMessage != flagMessage && lastMessage.Headers.SequenceSize < message.Headers.SequenceNumber) {
                logger.Debug("The message has a sequence number which is larger than the sequence size: "+ message);
                return false;
            }
            return true;
        }

        protected override void DoAddMessage(IMessage message, MessageBarrier<SortedDictionary<int, IMessage>, int> barrier) {
            //add the message to the barrier, indexing it by its sequence number
            barrier.Messages.Add(message.Headers.SequenceNumber, message);
        }

        private static IMessage CreateFlagMessage(int sequenceNumber) {
            return MessageBuilder.WithPayload(sequenceNumber).SetSequenceNumber(sequenceNumber).Build();
        }

        private static int GetFirstKey(IDictionary<int, IMessage> messages) {
            int minKey = Int32.MaxValue;
            foreach(int key in messages.Keys) {
                if (key < minKey)
                    minKey = key;
            }
            return minKey;
        }

        private static int GetLastKey(IDictionary<int, IMessage> messages) {
            int maxKey = Int32.MinValue;
            foreach(int key in messages.Keys) {
                if(key > maxKey)
                    maxKey = key;
            }
            return maxKey;
        }
    }
}
