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
using System.Threading;
using Spring.Integration.Core;
using Spring.Integration.Selector;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Util;

namespace Spring.Integration.Channel {

    /// <summary>
    /// Simple implementation of a message channel. Each <see cref="IMessage"/> is placed in
    /// a <see cref="IBlockingQueue{T}"/> whose capacity may be specified upon construction.
    /// The capacity must be a positive integer value. For a zero-capacity version
    /// consider the <see cref="RendezvousChannel"/>
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class QueueChannel : AbstractPollableChannel {

        private readonly IBlockingQueue<IMessage> _queue;

        #region ctors

        /// <summary>
        /// Create a channel with the specified queue.
        /// </summary>
        public QueueChannel(IBlockingQueue<IMessage> queue) {
            AssertUtils.ArgumentNotNull(queue, "queue", "must not be null");
            _queue = queue;
        }

        /// <summary>
        /// Create a channel with the specified queue capacity.
        /// </summary>
        public QueueChannel(int capacity) {
            AssertUtils.IsTrue(capacity > 0, "The capacity must be a positive integer. " +
                                             "For a zero-capacity alternative, consider using a 'RendezvousChannel'.");

            _queue = new LinkedBlockingQueue<IMessage>(capacity);
        }

        /// <summary>
        /// Create a channel with "unbounded" queue capacity. The actual capacity value is
        /// <see cref="int.MaxValue"/>. Note that a bounded queue is recommended, since an
        /// unbounded queue may lead to OutOfMemoryErrors.
        /// </summary>
        public QueueChannel() : this(new LinkedBlockingQueue<IMessage>()) { }

        #endregion

        #region overrides

        public override IList<IMessage> Clear() {
            IList<IMessage> clearedMessages = new List<IMessage>();
            _queue.DrainTo(clearedMessages);
            return clearedMessages;
        }

        public override IList<IMessage> Purge(IMessageSelector selector) {
            if(selector == null) {
                return Clear();
            }

            // removing during iterate through an IEnumerable is not supported in .NET.
            // so we use a two step algorythm. first we add all message not accepted by the
            // the selector to a list and then we iterate through this list and remove the elements
            IList<IMessage> messagesToRemove = new List<IMessage>();
            foreach(IMessage message in _queue) {
                if (!selector.Accept(message)) {
                    messagesToRemove.Add(message);
                }
            }

            IList<IMessage> purgedMessages = new List<IMessage>();
            foreach(IMessage message in messagesToRemove) {
                if(_queue.Remove(message)) {
                    purgedMessages.Add(message);
                }
            }
            return purgedMessages;
        }

        protected override IMessage DoReceive(TimeSpan timeout) {
            try {
                if(timeout.TotalMilliseconds > 0) {
                    IMessage message;
                    return _queue.Poll(timeout, out message) == false ? null : message;
                }
                if(timeout.TotalMilliseconds == 0) {
                    IMessage message;
                    return _queue.Poll(out message) == false ? null : message;
                }
                return _queue.Take();
            }
            catch(ThreadInterruptedException) {
                Thread.CurrentThread.Interrupt();
                return null;
            }
        }

        protected override bool DoSend(IMessage message, TimeSpan timeout) {
            AssertUtils.ArgumentNotNull(message, "'message' must not be null");
            try {
                if(timeout.TotalMilliseconds > 0) {
                    return _queue.Offer(message, timeout);
                }
                if(timeout == TimeSpan.Zero) {
                    return _queue.Offer(message);
                }
                _queue.Put(message);
                return true;
            }
            catch(ThreadInterruptedException) {
                Thread.CurrentThread.Interrupt();
                return false;
            }
        }

        #endregion

        /// <summary>
        /// TODO documentation
        /// </summary>
        public int MesssageCount {
            get { return _queue.Count; }
        }

        /// <summary>
        /// TODO documentation
        /// </summary>
        public int RemainingCapacity {
            get { return _queue.RemainingCapacity; }
        }
    }
}
