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

using System.Collections.Generic;
using Spring.Integration.Core;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// Strategy for determining when a group of messages reaches a state of
    /// completion (i.e. can trip a barrier).
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public interface ICompletionStrategy {

        /// <summary>
        /// check whether all <paramref name="messages"/> are completed
        /// </summary>
        /// <param name="messages">the messages to check</param>
        /// <returns><c>true</c> if all <paramref name="messages"/> are completed, otherwise <c>false</c></returns>
        bool IsComplete(IList<IMessage> messages);
    }
}
