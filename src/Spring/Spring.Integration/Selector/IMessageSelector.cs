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

using Spring.Integration.Core;

namespace Spring.Integration.Selector {
    /// <summary>
    /// Strategy interface for message selection.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public interface IMessageSelector {

        /// <summary>
        /// check whether the <paramref name="message"/> is accepted
        /// </summary>
        /// <param name="message">the message to check</param>
        /// <returns><c>true</c> if the message is accepted otherwise <c>false</c></returns>
        bool Accept(IMessage message);
    }
}