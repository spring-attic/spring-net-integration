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
using Spring.Integration.Core.Generic;

namespace Spring.Integration.Message.Generic {

    /// <summary>
    /// Base Message class defining common properties such as id, payload, and headers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [Serializable]
    public class Message<T> : Message, IMessage<T> {

        /// <summary>
        /// Create a new message with the given payload. The id will be generated by
        /// the default {@link IdGenerator} strategy.
        /// </summary>
        /// <param name="payload">the message payload</param>
        public Message(T payload)
            : base(payload) {
        }

        /// <summary>
        /// Create a new message with the given payload. The id will be generated by
        /// the default {@link IdGenerator} strategy. The headers will be populated
        /// with the provided header values.
        /// </summary>
        /// <param name="headers">message headers</param>
        /// <param name="payload">the message payload</param>
        public Message(T payload, IDictionary<string, object> headers)
            : base(payload, headers) {
        }

        #region IMessage<T> Members

        public T TypedPayload {
            get { return (T)Payload; }
        }

        #endregion
    }
}
