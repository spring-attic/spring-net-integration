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
using Spring.Integration.Core;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Aggregator {
    /// <summary>
    /// Base class for implementing adapters for methods which take as an argument a
    /// list of {@link Message Message} instances or payloads.
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageListMethodAdapter {

        private DefaultMethodInvoker _invoker;

        protected MethodInfo _method;


        public MessageListMethodAdapter(object obj, string methodName) {
            AssertUtils.ArgumentNotNull(obj, "'obj' must not be null");
            AssertUtils.ArgumentNotNull(methodName, "'methodName' must not be null");

            _method = GetMethodWithListParam(obj, methodName);
            AssertUtils.ArgumentNotNull(_method, "Method '" + methodName + "(List<> args)' not found on '" + obj.GetType() + "'.");
            _invoker = new DefaultMethodInvoker(obj, _method);
        }

        public MessageListMethodAdapter(object obj, MethodInfo method) {
            AssertUtils.ArgumentNotNull(obj, "'obj' must not be null");
            AssertUtils.ArgumentNotNull(method, "'method' must not be null");
            AssertUtils.IsTrue(method.GetParameters().Length == 1 && method.GetParameters()[0].ParameterType.Equals(typeof(IList<IMessage>)),
                    "Method must accept exactly one parameter, and it must be a List.");
            _method = method;
            _invoker = new DefaultMethodInvoker(obj, _method);
        }


        protected MethodInfo Method {
            get { return _method; }
        }

        private static bool IsActualTypeParameterizedMessage(MethodInfo method) {
            return GetCollectionActualType(method).IsGenericType && typeof(IMessage).IsAssignableFrom(GetCollectionActualType(method));
        }

        public object ExecuteMethod(IList<IMessage> messages) {
            try {
                if(IsMethodParameterParameterized(_method) && IsHavingActualTypeArguments(_method)
                        && (IsActualTypeRawMessage(_method) || IsActualTypeParameterizedMessage(_method))) {
                    return _invoker.InvokeMethod(messages);
                }
                return _invoker.InvokeMethod(ExtractPayloadsFromMessages(messages));
            }
            catch(TargetInvocationException e) {
                throw new MessagingException("Method '" + _method + "' threw an Exception.", e.InnerException);
            }
            catch(Exception e) {
                throw new MessagingException("Failed to invoke method '" + _method + "'.", e);
            }
        }

        private static IList<object> ExtractPayloadsFromMessages(IEnumerable<IMessage> messages) {
            IList<object> payloadList = new List<object>();
            foreach(IMessage message in messages) {
                payloadList.Add(message.Payload);
            }
            return payloadList;
        }

        private static bool IsActualTypeRawMessage(MethodInfo method) {
            return GetCollectionActualType(method).Equals(typeof(IMessage));
        }

        private static Type GetCollectionActualType(MethodInfo method) {
            return method.GetParameters()[0].ParameterType.GetGenericArguments()[0];
        }

        private static bool IsHavingActualTypeArguments(MethodInfo method) {
            return method.GetParameters()[0].ParameterType.GetGenericArguments().Length == 1;
        }

        private static bool IsMethodParameterParameterized(MethodInfo method) {
            return method.GetParameters()[0].ParameterType.IsGenericType;
        }

        /// <summary>
        /// try to find a method with name <paramref name="methodName"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static MethodInfo GetMethodWithListParam(object obj, string methodName) {

            foreach(MethodInfo mi in obj.GetType().GetMethods()) {
                if(!mi.Name.Equals(methodName))
                    continue;
                
                ParameterInfo[] pis = mi.GetParameters();
                if (pis.Length != 1)
                    continue;
                Type type = pis[0].ParameterType;

                if(!type.IsGenericType)
                    continue;

                if(type.Name.Equals("IList`1"))
                    return mi;
            }
            
            return null;
        }
    }
}
