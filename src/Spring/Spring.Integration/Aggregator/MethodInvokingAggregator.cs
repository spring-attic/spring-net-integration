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
using System.Reflection;
using Spring.Aop.Framework;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message.Generic;
using Spring.Threading.AtomicTypes;
using Spring.Util;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// {@link AbstractMessageAggregator} adapter for methods annotated with
    /// {@link Aggregator @Aggregator} annotation and for <code>aggregator</code>
    /// elements (e.g. &lt;aggregator ref="beanReference" method="methodName"/&gt;).
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MethodInvokingAggregator : AbstractMessageAggregator {

        private readonly MessageListMethodAdapter _methodInvoker;


        public MethodInvokingAggregator(object obj, MethodInfo method) {
            _methodInvoker = new MessageListMethodAdapter(obj, method);
        }

        public MethodInvokingAggregator(object obj, string methodName) {
            _methodInvoker = new MessageListMethodAdapter(obj, methodName);
        }

        public MethodInvokingAggregator(object obj) {
            AssertUtils.ArgumentNotNull(obj, "object must not be null");
            MethodInfo method = FindAggregatorMethod(obj);
            AssertUtils.ArgumentNotNull(method, "unable to resolve Aggregator method on target class [" + obj.GetType() + "]");
            _methodInvoker = new MessageListMethodAdapter(obj, method);
        }


        public override IMessage AggregateMessages(IList<IMessage> messages) {
            if(messages == null || messages.Count == 0) {
                return null;
            }
            object returnedValue = _methodInvoker.ExecuteMethod(messages);
            if(returnedValue == null) {
                return null;
            }
            if(returnedValue is IMessage) {
                return (IMessage)returnedValue;
            }
            return new Message<object>(returnedValue);
        }


        private static MethodInfo FindAggregatorMethod(object candidate) {
            Type targetClass = AopUtils.GetTargetType(candidate);
            if(targetClass == null) {
                targetClass = candidate.GetType();
            }
            MethodInfo method = FindAnnotatedMethod(targetClass);
            if(method == null) {
                method = FindSinglePublicMethod(targetClass);
            }
            return method;
        }

        private static MethodInfo FindAnnotatedMethod(Type targetClass) {
            AtomicReference<MethodInfo> annotatedMethod = new AtomicReference<MethodInfo>();

            MethodInfo[] methods = targetClass.GetMethods();
            foreach(MethodInfo method in methods) {

                object[] attributes = method.GetCustomAttributes(typeof(AggregatorAttribute), true);
                if(attributes.Length >= 1) {
                    if(annotatedMethod.Value != null)
                        throw new ArgumentException("found more than one method on target class [" + targetClass + "] with the annotation type [" + typeof(AggregatorAttribute).Name + "]");

                    annotatedMethod.Value = method;
                }
            }

            return annotatedMethod.Value;
        }

        private static MethodInfo FindSinglePublicMethod(Type targetClass) {
            MethodInfo result = null;
            foreach(MethodInfo method in targetClass.GetMethods()) {
                if(!method.DeclaringType.Equals(typeof(object))) {
                    if(result != null) {
                        throw new ArgumentException("Class [" + targetClass + "] contains more than one public Method.");
                    }
                    result = method;
                }
            }
            return result;
        }
    }
}
