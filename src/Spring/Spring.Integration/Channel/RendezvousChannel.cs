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
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A zero-capacity version of {@link QueueChannel} that delegates to a
    /// {@link SynchronousQueue} internally. This accommodates "handoff" scenarios
    /// (i.e. blocking while waiting for another party to send or receive).
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class RendezvousChannel : QueueChannel {

        /// <summary>
        /// create a new <see cref="RendezvousChannel"/> based on a <see cref="SynchronousQueue{T}"/>
        /// </summary>
        public RendezvousChannel()
            : base(new SynchronousQueue<IMessage>()) {
        }
    }
}
