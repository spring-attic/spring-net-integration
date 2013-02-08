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
using Spring.Integration.Core;

namespace Spring.Integration.Message {
    /// <summary>
    /// Exception that indicates an error during message handling.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageHandlingException : MessagingException {

        /// <summary>
        /// create a new <see cref="MessageHandlingException"/> with the <paramref name="failedMessage"/>
        /// </summary>
        /// <param name="failedMessage">the message which could not be delivered</param>
        public MessageHandlingException(IMessage failedMessage)
            : base(failedMessage) {
        }

        /// <summary>
        /// create a new <see cref="MessageHandlingException"/> with the <paramref name="failedMessage"/>,
        /// and <paramref name="description"/>
        /// </summary>
        /// <param name="failedMessage">the message which could not be delivered</param>
        /// <param name="description">a description</param>
        public MessageHandlingException(IMessage failedMessage, string description)
            : base(failedMessage, description) {
        }

        /// <summary>
        /// create a new <see cref="MessageHandlingException"/> with the <paramref name="failedMessage"/>,
        /// and <paramref name="innerException"/>
        /// </summary>
        /// <param name="failedMessage">the message which could not be delivered</param>
        /// <param name="innerException">the inner exception</param>
        public MessageHandlingException(IMessage failedMessage, Exception innerException)
            : base(failedMessage, innerException) {
        }

        /// <summary>
        /// create a new <see cref="MessageHandlingException"/> with the <paramref name="failedMessage"/>,
        /// <paramref name="description"/> and <paramref name="innerException"/>
        /// </summary>
        /// <param name="failedMessage">the message which could not be delivered</param>
        /// <param name="description">a description</param>
        /// <param name="innerException">the inner exception</param>
        public MessageHandlingException(IMessage failedMessage, string description, Exception innerException)
            : base(failedMessage, description, innerException) {
        }
    }
}
