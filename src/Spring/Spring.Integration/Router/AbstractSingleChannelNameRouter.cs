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

namespace Spring.Integration.Router {
    /// <summary>
    /// Extends {@link AbstractChannelNameResolvingMessageRouter} to support router
    /// implementations that always return a single channel name (or null).
    /// the channel name(s) rather than {@link MessageChannel} instances.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractSingleChannelNameRouter : AbstractChannelNameResolvingMessageRouter {

        protected override string[] DetermineTargetChannelNames(IMessage message) {
            string channelName = DetermineTargetChannelName(message);
            return (channelName != null) ? new[] { channelName } : null;
        }

        /**
         * Subclasses must implement this method to return the channel name.
         */
        protected abstract string DetermineTargetChannelName(IMessage message);
    }
}
