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

namespace Spring.Integration.Core {

    /// <summary>
    /// Base channel interface defining common behavior for message sending and receiving.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public interface IMessageChannel {
        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Send a <see cref="IMessage"/> to this channel. May throw a RuntimeException for
        /// non-recoverable errors. Otherwise, if the Message cannot be sent for a
        /// non-fatal reason this method will return 'false', and if the Message is
        /// sent successfully, it will return 'true'.
        /// </summary>
        /// <param name="message">the <see cref="IMessage"/> to send</param>
        /// <returns>
        /// whether the Message has been sent successfully
        /// </returns>
        /// <remarks>
        /// Depending on the implementation, this method may block indefinitely.
        /// To provide a maximum wait time, use <see cref="Send(IMessage,TimeSpan)"/>.
        /// </remarks>
        bool Send(IMessage message);

        /// <summary>
        /// Send a message, blocking until either the message is accepted or the
        /// specified timeout period elapses.
        /// </summary>
        /// <param name="message">the <see cref="IMessage"/> to send</param>
        /// <param name="timeout">the timeout</param>
        /// <returns>
        /// 	<c>true</c> if the message is sent successfully,
        ///     <c>false</c> if the specified timeout period elapses or the send is interrupted
        /// </returns>
        bool Send(IMessage message, TimeSpan timeout);
    }
}
