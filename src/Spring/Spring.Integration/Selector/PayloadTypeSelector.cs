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

using System;
using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Selector {
    /// <summary>
    /// A {@link MessageSelector} implementation that checks the type of the
    /// {@link Message} payload. The payload type must be assignable to at least one
    /// of the selector's accepted types.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PayloadTypeSelector : IMessageSelector {

        private readonly IList<Type> _acceptedTypes = new List<Type>();

        /// <summary>
        /// Create a selector for the provided types. At least one is required.
        /// </summary>
        /// <param name="types">the accepted types</param>
        public PayloadTypeSelector(params Type[] types) {
            AssertUtils.ArgumentNotNull(types, "at least one type is required");
            foreach(Type type in types) {
                _acceptedTypes.Add(type);
            }
        }

        public bool Accept(IMessage message) {
            AssertUtils.ArgumentNotNull(message, "'message' must not be null");
            object payload = message.Payload;
            AssertUtils.ArgumentNotNull(payload, "'payload' must not be null");
            foreach(Type type in _acceptedTypes) {
                if(type.IsAssignableFrom(payload.GetType())) {
                    return true;
                }
            }
            return false;
        }
    }
}
