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
using Spring.Integration.Selector;

namespace Spring.Integration.Channel {
    /// <summary>
    /// Base class for all pollable channels.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractPollableChannel : AbstractMessageChannel, IPollableChannel {

        #region IPollableChannel Members

        /// <summary>
        /// Receive the first available message from this channel. If the channel
        /// contains no messages, this method will block.
        /// </summary>        
        /// <returns>
        /// the first available message or <code>null</code> if the
        /// receiving thread is interrupted.
        /// </returns>
        public IMessage Receive() {
            return Receive(new TimeSpan(-1));
        }

        /// <summary>
        /// Receive the first available message from this channel. If the channel
        /// contains no messages, this method will block until the allotted timeout
        /// elapses. If the specified timeout is 0, the method will return
        /// immediately. If less than zero, it will block indefinitely (see
        /// {@link #receive()}).
        /// </summary>
        /// <param name="timeout">the timeout in milliseconds</param>
        /// <returns>
        /// the first available message or <code>null</code> if no message
        /// is available within the allotted time or the receiving thread is
        /// interrupted.
        /// </returns>
        public IMessage Receive(TimeSpan timeout) {
            if(!Interceptors.PreReceive(this)) {
                return null;
            }
            IMessage message = DoReceive(timeout);
            message = Interceptors.PostReceive(message, this);
            return message;
        }

        public abstract IList<IMessage> Clear();
        public abstract IList<IMessage> Purge(IMessageSelector selector);

        #endregion

        /// <summary>
        /// Subclasses must implement this method. A non-negative timeout indicates
        /// how long to wait if the channel is empty (if the value is 0, it must
        /// return immediately with or without success). A negative timeout value
        /// indicates that the method should block until either a message is
        /// available or the blocking thread is interrupted.
        /// </summary>    
        protected abstract IMessage DoReceive(TimeSpan timeout);
    }
}