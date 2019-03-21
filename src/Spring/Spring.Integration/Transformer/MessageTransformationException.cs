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
using Spring.Integration.Core;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// Base Exception type for Message transformation errors.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageTransformationException : MessagingException {

        public MessageTransformationException(IMessage message, string description, Exception cause)
            : base(message, description, cause) { }

        public MessageTransformationException(IMessage message, string description)
            : base(message, description) { }

        public MessageTransformationException(IMessage message, Exception cause)
            : base(message, cause) { }

        public MessageTransformationException(string description, Exception cause)
            : base(description, cause) { }

        public MessageTransformationException(string description)
            : base(description) { }
    }
}
