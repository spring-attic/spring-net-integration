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
using Spring.Integration.Core;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// Utility class for AbstractMessageBarrierHandler and its subclasses for
    /// storing objects while in transit. It is a wrapper around a {@link Map},
    /// providing special properties for recording the complete status, the creation
    /// time (for determining if a group of messages has timed out), and the
    /// correlation id for a group of messages (available after the first message has
    /// been added to it). This is a parameterized type, allowing different different
    /// client classes to use different types of Maps and their respective features.
    /// 
    /// This class is not thread-safe and will be synchronized by the calling code.
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageBarrier<T, K> where T : IDictionary<K, IMessage> {

        protected T _messages;

        private volatile bool _complete;

        private readonly DateTime _timestamp = DateTime.Now;

        /// <summary>
        /// create a new intsance of MessageBarrier
        /// </summary>
        /// <param name="messages">the messages</param>
        public MessageBarrier(T messages) {
            _messages = messages;
        }

        /// <summary>
        /// get the correlation id of the first message
        /// </summary>
        public object CorrelationId {
            get {
                if(_messages.Count > 0) {
                    IEnumerator<IMessage> enumerator = _messages.Values.GetEnumerator();
                    enumerator.MoveNext();
                    return enumerator.Current.Headers.CorrelationId;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the creation time of this barrier 
        /// </summary>
        public DateTime Timestamp {
            get { return _timestamp; }
        }

        /// <summary>
        /// Marks the barrier as complete.
        /// </summary>
        public void SetComplete() {
            _complete = true;
        }

        /// <summary>
        /// True if the barrier has received all the messages and can proceed to release them.
        /// </summary>
        /// <returns></returns>
        public bool IsComplete {
            get { return _complete; }
        }

        /// <summary>
        /// get the messages of this barrier
        /// </summary>
        public T Messages {
            get { return _messages; }
        }
    }
}
