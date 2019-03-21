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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Spring.Integration.Core;

namespace Spring.Integration.Router {
    /// <summary>
    /// Extends {@link AbstractMessageRouter} to support router implementations that
    /// always return a single {@link MessageChannel} instance (or null).
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractSingleChannelRouter : AbstractMessageRouter {

        protected override ICollection<IMessageChannel> DetermineTargetChannels(IMessage message) {
            IMessageChannel channel = DetermineTargetChannel(message);
            if (channel == null)
                return null;

            IList<IMessageChannel> list = new List<IMessageChannel>();
            list.Add(channel);

            return new ReadOnlyCollection<IMessageChannel>(list);
        }

        /**
         * Subclasses must implement this method to return the target channel.
         */
        protected abstract IMessageChannel DetermineTargetChannel(IMessage message);
    }
}
