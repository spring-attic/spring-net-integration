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

namespace Spring.Integration.Attributes {
    /// <summary>
    /// Indicates that a method is capable of resolving to a channel or channel name
    /// based on a message, message header(s), or both.
    /// <p>
    /// A method annotated with @Router may accept a parameter of type
    /// {@link org.springframework.integration.core.Message} or of the expected
    /// Message payload's type. Any type conversion supported by
    /// {@link org.springframework.beans.SimpleTypeConverter} will be applied to
    /// the Message payload if necessary. Header values can also be passed as
    /// Message parameters by using the {@link Header @Header} parameter annotation.
    /// <p>
    /// Return values from the annotated method may be either a Collection or Array
    /// whose elements are either
    /// {@link org.springframework.integration.core.MessageChannel channels} or
    /// Strings. In the latter case, the endpoint hosting this router will attempt
    /// to resolve each channel name with the Channel Registry.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [AttributeUsage(AttributeTargets.Method)]
    public class RouterAttribute : Attribute {
        private string _inputChannel = String.Empty;
        private string _defaultOutputChannel = String.Empty;

        public string InputChannel {
            get { return _inputChannel; }
            set { _inputChannel = value; }
        }

        public string DefaultOutputChannel {
            get { return _defaultOutputChannel; }
            set { _defaultOutputChannel = value; }
        }
    }
}
