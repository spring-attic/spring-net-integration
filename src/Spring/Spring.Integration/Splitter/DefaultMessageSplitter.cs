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

using Spring.Integration.Core;

namespace Spring.Integration.Splitter {
    /// <summary>
    /// The default Message Splitter implementation. Returns individual Messages
    /// after receiving an array or Collection. If a value is provided for the
    /// 'delimiters' property, then String payloads will be tokenized based on
    /// those delimiters.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class DefaultMessageSplitter : AbstractMessageSplitter {

        private volatile string _delimiters;

        /// <summary>
        /// Set delimiters to use for tokenizing String values. The default is
        /// <code>null</code> indicating that no tokenization should occur. If
        /// delimiters are provided, they will be applied to any String payload.
        /// </summary>
        public string Delimiters {
            set { _delimiters = value; }
        }

        protected override object SplitMessage(IMessage message) {
            object payload = message.Payload;
            if(payload is string && _delimiters != null) {
                return ((string)payload).Split(_delimiters.ToCharArray());
            }
            return payload;
        }
    }
}
