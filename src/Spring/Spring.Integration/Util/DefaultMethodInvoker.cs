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
using Spring.Objects.Support;
using Spring.Util;

namespace Spring.Integration.Util {

    /// <summary>
    /// Implementation of {@link MethodInvoker} to be used when the actual {@link Method} reference is known.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class DefaultMethodInvoker : IMethodInvoker {

        private readonly object _obj;

        private volatile MethodInfo _method;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        public DefaultMethodInvoker(object obj, MethodInfo method) {
            AssertUtils.ArgumentNotNull(obj, "object must not be null");
            AssertUtils.ArgumentNotNull(method, "method must not be null");
            _obj = obj;
            _method = method;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public object InvokeMethod(params object[] args) {

            ParameterInfo[] pi = _method.GetParameters();
            if(pi.Length != args.Length) {
                throw new ArgumentException("Wrong number of arguments. Expected types " + ArrayUtils.ToString(pi) + ", but received values " + ArrayUtils.ToString(args) + ".");
            }

            ArgumentConvertingMethodInvoker helper = new ArgumentConvertingMethodInvoker();
            helper.TargetObject = _obj;
            helper.TargetMethod = _method.Name;
            helper.Arguments = args;
            helper.Prepare();
            return helper.Invoke();
        }
    }
}
