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

using System.Reflection;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// An implementation of {@link HandlerMethodResolver} that always returns the
    /// same Method instance. Used when the exact Method is indicated explicitly
    /// or otherwise resolvable in advance based on static metadata.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class StaticHandlerMethodResolver : IHandlerMethodResolver {

        private readonly MethodInfo _method;

        /// <summary>
        /// create a <see cref="StaticHandlerMethodResolver"/> with handling method <paramref name="method"/>
        /// </summary>
        /// <param name="method">the handling method</param>
        public StaticHandlerMethodResolver(MethodInfo method) {
            AssertUtils.ArgumentNotNull(method, "method must not be null");
            AssertUtils.ArgumentHasElements(method.GetParameters(), "Message-handling method [" + method + "] must accept at least one parameter.");
            AssertUtils.IsTrue(HandlerMethodUtils.IsValidHandlerMethod(method), "Invalid Message-handling method [" + method + "]");

            _method = method;
        }

        public MethodInfo ResolveHandlerMethod(IMessage message) {
            return _method;
        }
    }
}
