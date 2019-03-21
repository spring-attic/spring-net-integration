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
    /// The base generic exception for any failures within the messaging system.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessagingException : ApplicationException {

        private readonly IMessage _failedMessage;

        /// <summary>
        /// create a new <see cref="MessagingException"/> from the causing message 
        /// </summary>
        /// <param name="message">the message which cause the exception</param>
        public MessagingException(IMessage message) {
            _failedMessage = message;
        }

        /// <summary>
        /// create a new <see cref="MessagingException"/> with a description 
        /// </summary>
        /// <param name="description">the exception description</param>
        public MessagingException(string description)
            : base(description) {

        }

        /// <summary>
        /// create a new <see cref="MessagingException"/> with a description and an inner exception
        /// </summary>
        /// <param name="description">the exception description</param>
        /// <param name="innerException">the inner exception</param>
        public MessagingException(string description, Exception innerException)
            : base(description, innerException) {

        }

        /// <summary>
        /// create a new <see cref="MessagingException"/> with the causing message and a description</code>
        /// </summary>
        /// <param name="message">the causing message</param>
        /// <param name="description">the exception description</param>
        public MessagingException(IMessage message, string description) : base(description) {
            _failedMessage = message;
        }

        /// <summary>
        /// create a new <see cref="MessagingException"/> with the causing message and an inner exception</code>
        /// </summary>
        /// <param name="message">the causing message</param>
        /// <param name="innerException">the inner exception</param>
        public MessagingException(IMessage message, Exception innerException) : base("", innerException) {
            _failedMessage = message;
        }

        /// <summary>
        /// create a new <see cref="MessagingException"/> with the causing message, description and an inner exception</code>
        /// </summary>
        /// <param name="message">the causing message</param>
        /// <param name="description">the exception description</param>
        /// <param name="innerException">the inner exception</param>
        public MessagingException(IMessage message, string description, Exception innerException) : base(description, innerException) {
            _failedMessage = message;
        }

        /// <summary>
        /// get the message which cause the exception
        /// </summary>
        public IMessage FailedMessage {
            get { return _failedMessage; }
        }
    }
}
