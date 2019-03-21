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

using System.Collections.Generic;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class Adder
    {
        // TODO this should be IList<long> but then we need complete generic channels. problem is here MethodListMethodAdapter.ExtractPayloadFromMessages
        // which produces an IList<object> which then is provided as a parameter for this method
        public long Add(IList<object> results)
        {
            long total = 0l;
            foreach (long partialResult in results)
            {
                total += partialResult;
            }
            return total;
        }
    }
}