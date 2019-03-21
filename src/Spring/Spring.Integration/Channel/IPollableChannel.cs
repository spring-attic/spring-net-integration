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

namespace Spring.Integration.Channel {
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public interface IPollableChannel : IMessageChannel {

        /// <summary>
        /// Receive a message from this channel, blocking indefinitely if necessary.
        /// </summary>
        /// <returns>
        /// the next available <see cref="IMessage"/> or <c>null</c> if interrupted
        /// </returns>
        IMessage Receive();

        /// <summary>
        /// Receive a message from this channel, blocking until either a message is
        /// available or the specified timeout period elapses.
        /// </summary>
        /// <param name="timeout">the timeout</param>
        /// <returns>
        /// the next available <see cref="IMessage"/> or <c>null</c> if the
        /// specified timeout period elapses or the message reception is interrupted
        /// </returns>
        IMessage Receive(TimeSpan timeout);

        /// <summary>
        /// Remove all <see cref="IMessage"/> messages from this channel.
        /// </summary>
        IList<IMessage> Clear();

        /// <summary>
        /// Remove any <see cref="IMessage"/> messages that are not accepted by the provided selector.
        /// </summary>
        IList<IMessage> Purge(IMessageSelector selector);
    }
}
