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
    /// An implementation of {@link CompletionStrategy} that simply compares the
    /// current size of the message list to the expected 'sequenceSize' according to
    /// the first {@link Message} in the list.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class SequenceSizeCompletionStrategy : ICompletionStrategy {

        public bool IsComplete(IList<IMessage> messages) {
            if(messages == null || messages.Count == 0) {
                return false;
            }
            return messages.Count != 0 && (messages.Count >= messages[0].Headers.SequenceSize);
        }
    }
}
