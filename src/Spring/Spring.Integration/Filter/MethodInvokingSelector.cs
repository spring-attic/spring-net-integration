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
using System.Reflection;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Selector;
using Spring.Util;

namespace Spring.Integration.Filter {
    /// <summary>
    /// A method-invoking implementation of {@link MessageSelector}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MethodInvokingSelector : IMessageSelector {

        private MessageMappingMethodInvoker _invoker;


        public MethodInvokingSelector(object obj, MethodInfo method) {
            _invoker = new MessageMappingMethodInvoker(obj, method);
            Type returnType = method.ReturnType;
            AssertUtils.IsTrue(typeof(bool).IsAssignableFrom(returnType), "MethodInvokingSelector method must return a boolean result.");
        }

        public MethodInvokingSelector(object obj, string methodName) {
            _invoker = new MessageMappingMethodInvoker(obj, methodName);
        }


        public bool Accept(IMessage message) {
            object result = _invoker.InvokeMethod(message);
            AssertUtils.ArgumentNotNull(result, "result must not be null");
            if(!typeof(bool).IsAssignableFrom(result.GetType()))
                throw new ArgumentException("a boolean result is required");
            return (bool)result;
        }
    }
}
