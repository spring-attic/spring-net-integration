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
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A message channel that prioritizes messages based on a {@link Comparator}.
    /// The default comparator is based upon the message header's 'priority'.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class PriorityChannel : QueueChannel {

        private readonly Semaphore _semaphore;


        /// <summary>
        /// Create a channel with the specified queue capacity. If the capacity
        /// is a non-positive value, the queue will be unbounded. Message priority
        /// will be determined by the provided {@link Comparator}. If the comparator
        /// is <code>null</code>, the priority will be based upon the value of
        /// {@link MessageHeader#getPriority()}.
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="comparator"></param>
        public PriorityChannel(int capacity, IComparer<IMessage> comparator)
            : base(new PriorityBlockingQueue<IMessage>(11, (comparator != null) ? comparator : new MessagePriorityComparator())) {
            _semaphore = (capacity > 0) ? new Semaphore(capacity, capacity) : null;
        }

        /// <summary>
        /// Create a channel with the specified queue capacity. Message priority
        /// will be based upon the value of {@link MessageHeader#getPriority()}.
        /// </summary>
        /// <param name="capacity"></param>
        public PriorityChannel(int capacity)
            : this(capacity, null) {
        }

        /// <summary>
        /// Create a channel with an unbounded queue. Message priority will be
        /// determined by the provided {@link Comparator}. If the comparator
        /// is <code>null</code>, the priority will be based upon the value of
        /// {@link MessageHeader#getPriority()}.
        /// </summary>
        /// <param name="comparator"></param>
        public PriorityChannel(IComparer<IMessage> comparator)
            : this(0, comparator) {
        }

        /// <summary>
        /// Create a channel with an unbounded queue. Message priority will be
        /// based on the value of {@link MessageHeader#getPriority()}.
        /// </summary>
        public PriorityChannel()
            : this(0, null) {
        }


        protected override bool DoSend(IMessage message, TimeSpan timeout) {
            if(!AcquirePermitIfNecessary(timeout)) {
                return false;
            }
            return base.DoSend(message, TimeSpan.Zero);
        }

        protected override IMessage DoReceive(TimeSpan timeout) {
            IMessage message = base.DoReceive(timeout);
            if(message != null) {
                ReleasePermitIfNecessary();
                return message;
            }
            return null;
        }

        private bool AcquirePermitIfNecessary(TimeSpan timeout) {
            if(_semaphore != null) {
                try {
                    return _semaphore.WaitOne(timeout);
                }
                catch(ThreadInterruptedException) {
                    Thread.CurrentThread.Interrupt();
                    return false;
                }
            }
            return true;
        }

        private void ReleasePermitIfNecessary() {
            if(_semaphore != null) {
                _semaphore.Release();
            }
        }

        private class MessagePriorityComparator : IComparer<IMessage> {

            public int Compare(IMessage message1, IMessage message2) {
                MessagePriority priority1 = message1.Headers.Priority;
                MessagePriority priority2 = message2.Headers.Priority;
                priority1 = priority1 != 0 ? priority1 : MessagePriority.NORMAL;
                priority2 = priority2 != 0 ? priority2 : MessagePriority.NORMAL;
                return priority1.CompareTo(priority2);
            }
        }
    }
}
