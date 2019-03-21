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

using Spring.Integration.Dispatcher;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A channel that invokes a single subscriber for each sent Message.
    /// The invocation will occur in the sender's thread.
    /// </summary>
    /// <author>Dave Syer</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class DirectChannel : AbstractSubscribableChannel<SimpleDispatcher> {

        /// <summary>
        /// create a new <see cref="DirectChannel"/> using a <see cref="SimpleDispatcher"/>
        /// </summary>
        public DirectChannel()
            : base(new SimpleDispatcher()) {
        }
    }
}
