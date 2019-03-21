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

using Spring.Objects.Support;
using Spring.Util;

namespace Spring.Integration.Util {

    /// <summary>
    /// Implementation of {@link MethodInvoker} to be used when only the method name is known.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class NameResolvingMethodInvoker : IMethodInvoker {

        private readonly object _obj;

        private readonly string _methodName;

        private volatile IMethodValidator _methodValidator;

        /// <summary>
        /// create a new <see cref="NameResolvingMethodInvoker"/>
        /// </summary>
        /// <param name="obj">the object to call the mthod on</param>
        /// <param name="methodName">the name of the method to call</param>
        public NameResolvingMethodInvoker(object obj, string methodName) {
            AssertUtils.ArgumentNotNull(obj, "'obj' must not be null");
            AssertUtils.ArgumentNotNull(methodName, "'methodName' must not be null");
            _obj = obj;
            _methodName = methodName;
        }

        /// <summary>
        /// set a <see cref="IMethodValidator"/> for the method
        /// </summary>
        public IMethodValidator MethodValidator {
            set { _methodValidator = value; }
        }

        /// <summary>
        /// invoke the method converting the <paramref name="args"/> if necessary to the appropriate type
        /// </summary>
        /// <param name="args">the arguments for the method</param>
        /// <returns>the return value of the method</returns>
        public object InvokeMethod(params object[] args) {
            ArgumentConvertingMethodInvoker invoker = new ArgumentConvertingMethodInvoker();
            invoker.TargetObject = _obj;
            invoker.TargetMethod = _methodName;
            invoker.Arguments = args;
            invoker.Prepare();
            //invoker.getPreparedMethod().setAccessible(true);
            if(_methodValidator != null) {
                _methodValidator.Validate(invoker.GetPreparedMethod());
            }
            return invoker.Invoke();
        }
    }
}
