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

namespace Spring.Integration.Channel {

    /// <summary>
    /// Strategy for resolving a name to a <see cref="IMessageChannel"/>
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public interface IChannelResolver {

        /// <summary>
        /// Return the MessageChannel for the given name.
        /// </summary>
        /// <param name="channelName">the name to resolve</param>
        /// <returns>the resolved channel</returns>
        IMessageChannel ResolveChannelName(string channelName);
    }
}