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
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Message {
    /// <summary>
    /// A {@link MessageSource} implementation that invokes a no-argument method so
    /// that its return value may be sent to a channel.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MethodInvokingMessageSource : IMessageSource, IInitializingObject {

        private volatile object obj;

        private volatile MethodInfo method;

        private volatile string methodName;

        private volatile IMethodInvoker invoker;


        /// <summary>
        /// set the object on which the method is called
        /// </summary>
        public object Object {
            set {
                AssertUtils.ArgumentNotNull(value, "Object", "must not be null");
                obj = value;
            }
        }

        /// <summary>
        /// set the method to call
        /// </summary>
        public MethodInfo Method {
            set {
                AssertUtils.ArgumentNotNull(method, "Method", "must not be null");
                method = value;
                methodName = value.Name;
            }
        }

        /// <summary>
        /// set the name of the method to call
        /// </summary>
        public string MethodName {
            set {
                AssertUtils.ArgumentNotNull(value, "MethodName", "must not be null");
                methodName = value;
            }
        }

        public void AfterPropertiesSet() {
            if(method != null) {
                invoker = new DefaultMethodInvoker(obj, method);
            }
            else if(methodName != null) {
                NameResolvingMethodInvoker nrmi = new NameResolvingMethodInvoker(obj, methodName);
                nrmi.MethodValidator = new MessageReceivingMethodValidator();
                invoker = nrmi;
            }
            else {
                throw new ArgumentException("either 'method' or 'methodName' is required");
            }
        }

        public IMessage Receive() {
            if(invoker == null) {
                AfterPropertiesSet();
            }
            try {
                object result = invoker.InvokeMethod(new object[] { });
                if(result == null) {
                    return null;
                }
                if(result is IMessage) {
                    return (IMessage)result;
                }
                return new Message(result);
            }
            catch(TargetInvocationException ex) {
                throw new MessagingException("Source method '" + methodName + "' threw an Exception.", ex.GetBaseException());
            }
            catch(Exception ex) {
                throw new MessagingException("Failed to invoke source method '" + methodName + "'.", ex);
            }
        }


        private class MessageReceivingMethodValidator : IMethodValidator {

            public void Validate(MethodInfo method) {
                if(method.ReturnType.Equals(typeof(void)))
                    throw new ArgumentException("MethodInvokingSource requires a non-void returning method.");
            }
        }
    }
}
