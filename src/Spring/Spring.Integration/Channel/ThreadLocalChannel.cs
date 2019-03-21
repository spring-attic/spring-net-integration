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
using Spring.Integration.Selector;
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A channel implementation that stores messages in a thread-bound queue. In
    /// other words, send() will put a message at the tail of the queue for the
    /// current thread, and receive() will retrieve a message from the head of the
    /// queue.
    /// </summary>
    /// <author>Dave Syer</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class ThreadLocalChannel : AbstractPollableChannel {

        [ThreadStatic]
        private static LinkedBlockingQueue<IMessage> _queue = new LinkedBlockingQueue<IMessage>();

        protected override IMessage DoReceive(TimeSpan timeout) {
            IMessage result;
            return _queue.Poll(out result) ? result : null;
        }

        protected override bool DoSend(IMessage message, TimeSpan timeout) {
            if(message == null) {
                return false;
            }
            // TODO check success of sending the message
            _queue.Add(message);
            return true;
        }

        /// <summary>
        /// Remove and return any messages that are stored for the current thread.
        /// </summary>
        /// <returns></returns>
        public override IList<IMessage> Clear() {
            IList<IMessage> removedMessages = new List<IMessage>();
            IMessage next;
            while(_queue.Poll(out next)) {
                removedMessages.Add(next);
            }
            return removedMessages;
        }

        /// <summary>
        /// Remove and return any messages that are stored for the current thread
        /// and do not match the provided <paramref name="selector"/>.
        /// </summary>
        /// <param name="selector">the message selector</param>
        /// <returns>the removed messages</returns>
        public override IList<IMessage> Purge(IMessageSelector selector) {
            IList<IMessage> removedMessages = new List<IMessage>();
            IMessage[] allMessages = _queue.ToArray();
            foreach(IMessage message in allMessages) {
                if(!selector.Accept(message) && _queue.Remove(message)) {
                    removedMessages.Add(message);
                }
            }
            return removedMessages;
        }
    }
}
