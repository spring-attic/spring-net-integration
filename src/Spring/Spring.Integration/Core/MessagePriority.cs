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

namespace Spring.Integration.Core {

    /// <summary>
    /// An enumeration of the possible values for a message's priority. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    /// <see cref="MessageHeaders.Priority"/>
    public enum MessagePriority {
        /// <summary>
        /// the highest priority. 
        /// <remarks>is initialized to 1 to identify an unset priority which is default(MessagePriority) which is 0</remarks>
        /// </summary>
        HIGHEST=1,
        /// <summary>
        /// high priority
        /// </summary>
        HIGH,
        /// <summary>
        /// normal priority. this is the default
        /// </summary>
        NORMAL,
        /// <summary>
        /// low priority
        /// </summary>
        LOW,
        /// <summary>
        /// the lowest priority
        /// </summary>
        LOWEST
    }
}
