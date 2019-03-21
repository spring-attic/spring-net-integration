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
using System.Reflection;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// Adapter for methods annotated with
    /// {@link org.springframework.integration.annotation.CompletionStrategy @CompletionStrategy}
    /// and for '<code>completion-strategy</code>' elements that include a '<code>method</code>'
    /// attribute (e.g. &lt;completion-strategy ref="beanReference" method="methodName"/&gt;).
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class CompletionStrategyAdapter : MessageListMethodAdapter, ICompletionStrategy {

        public CompletionStrategyAdapter(object obj, MethodInfo method)
            : base(obj, method) {
            AssertMethodReturnsBoolean();
        }

        public CompletionStrategyAdapter(object obj, string methodName)
            : base(obj, methodName) {
            AssertMethodReturnsBoolean();
        }


        public bool IsComplete(IList<IMessage> messages) {
            return ((bool)ExecuteMethod(messages)); // TODO .booleanValue();
        }

        private void AssertMethodReturnsBoolean() {
            AssertUtils.IsTrue(typeof(bool).Equals(Method.ReturnType), "Method '" + Method.Name + "' does not return a boolean value");
        }
    }
}
