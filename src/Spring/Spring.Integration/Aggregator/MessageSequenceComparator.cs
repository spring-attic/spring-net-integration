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
    /// A {@link Comparator} implementation based on the 'sequence number'
    /// property of a {@link Message Message's} header.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageSequenceComparator : IComparer<IMessage> {

        public int Compare(IMessage message1, IMessage message2) {
            int s1 = message1.Headers.SequenceNumber;
            int s2 = message2.Headers.SequenceNumber;

            return s1.CompareTo(s2);
        }
    }
}
