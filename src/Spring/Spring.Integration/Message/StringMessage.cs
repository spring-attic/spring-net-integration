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

using System.ComponentModel;
using Spring.Integration.Message.Generic;
using Spring.Integration.Message.TypeConverters;

namespace Spring.Integration.Message {
    /// <summary>
    /// A message implementation that accepts a <see cref="string"/> payload. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TypeConverter(typeof(StringMessageTypeConverter))]
    public class StringMessage : Message<string> {

        /// <summary>
        /// create a new <see cref="StringMessage"/> with a <paramref name="payload"/>
        /// </summary>
        /// <param name="payload">the payload for the message</param>
        public StringMessage(string payload)
            : base(payload) {
        }
    }
}
