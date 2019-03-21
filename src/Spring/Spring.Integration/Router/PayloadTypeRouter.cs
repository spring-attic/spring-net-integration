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
using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Router {
    /// <summary>
    /// A Message Router that resolves the {@link MessageChannel} based on the
    /// {@link Message Message's} payload type.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class PayloadTypeRouter : AbstractSingleChannelRouter {

        private volatile IDictionary<Type, IMessageChannel> _payloadTypeChannelMap = new Dictionary<Type, IMessageChannel>();//TODO  new ConcurrentHashMap<Class<?>, MessageChannel>();

        /// <summary>
        /// set the dictionary to map <see cref="Type"/>s to <see cref="IMessageChannel"/>s
        /// </summary>
        public IDictionary<Type, IMessageChannel> PayloadTypeChannelMap {
            set {
                AssertUtils.ArgumentNotNull(value, "payloadTypeChannelMap must not be null");
                _payloadTypeChannelMap = value;
            }
        }

        protected override IMessageChannel DetermineTargetChannel(IMessage message) {
            return DictionaryUtils.Get(_payloadTypeChannelMap, message.Payload.GetType());
        }
    }
}