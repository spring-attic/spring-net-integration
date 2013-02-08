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
    /// Exception that indicates an error during message delivery.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageDeliveryException : MessagingException {

        /// <summary>
        /// create a new <see cref="MessageDeliveryException"/> with the <paramref name="undeliveredMessage"/>
        /// </summary>
        /// <param name="undeliveredMessage">the message which could not be delivered</param>
        public MessageDeliveryException(IMessage undeliveredMessage)
            : base(undeliveredMessage) {
        }

        /// <summary>
        /// create a new <see cref="MessageDeliveryException"/> with the <paramref name="undeliveredMessage"/> 
        /// and <paramref name="description"/>
        /// </summary>
        /// <param name="undeliveredMessage">the message which could not be delivered</param>
        /// <param name="description">a description</param>
        public MessageDeliveryException(IMessage undeliveredMessage, string description)
            : base(undeliveredMessage, description) {
        }

        /// <summary>
        /// create a new <see cref="MessageDeliveryException"/> with the <paramref name="undeliveredMessage"/>,
        /// <paramref name="description"/> and <paramref name="innerException"/>
        /// </summary>
        /// <param name="undeliveredMessage">the message which could not be delivered</param>
        /// <param name="description">a description</param>
        /// <param name="innerException">the inner exception</param>
        public MessageDeliveryException(IMessage undeliveredMessage, string description, Exception innerException)
            : base(undeliveredMessage, description, innerException) {
        }
    }
}
