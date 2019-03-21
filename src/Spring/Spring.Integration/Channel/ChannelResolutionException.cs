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

using System;
using Spring.Integration.Core;

namespace Spring.Integration.Channel {
    /// <summary>
    /// Thrown by a ChannelResolver when it cannot resolve a channel name.
    /// <see cref="IChannelResolver{T}"/>
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class ChannelResolutionException : MessagingException {

        /// <summary>
        ///Create a new ChannelResolutionException.
        /// </summary>
        /// <param name="description">the description</param>
        public ChannelResolutionException(string description)
            : base(description) {
        }

        /// <summary>
        /// Create a new ChannelResolutionException.
        /// </summary>
        /// <param name="description">the description</param>
        /// <param name="innerException">the exception cause</param>
        public ChannelResolutionException(string description, Exception innerException)
            : base(description, innerException) {
        }
    }
}
