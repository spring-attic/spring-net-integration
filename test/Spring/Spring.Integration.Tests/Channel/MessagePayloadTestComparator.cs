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

#region

using System;
using System.Collections.Generic;
using Spring.Integration.Core;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class MessagePayloadTestComparator : IComparer<IMessage>
    {
        public int Compare(IMessage message1, IMessage message2)
        {
            IComparable c1 = message1.Payload as IComparable;
            IComparable c2 = message2.Payload as IComparable;
            if (c1 == null || c2 == null)
                throw new ArgumentException("both params must implement IComparable");

            return c1.CompareTo(c2);
        }
    }
}