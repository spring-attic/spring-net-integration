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
using System.Reflection;
using Spring.Integration.Core;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// An implementation of {@link HandlerMethodResolver} that matches the payload
    /// type of the Message against the expected type of its candidate methods.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PayloadTypeMatchingHandlerMethodResolver : IHandlerMethodResolver {

        private readonly IDictionary<Type, MethodInfo> _methodMap = new Dictionary<Type, MethodInfo>();

        private volatile MethodInfo _fallbackMethod;

        /// <summary>
        /// create a new <see cref="PayloadTypeMatchingHandlerMethodResolver"/> from <paramref name="candidates"/>
        /// </summary>
        /// <param name="candidates">the collectio of candidate methods</param>
        public PayloadTypeMatchingHandlerMethodResolver(ICollection<MethodInfo> candidates) {
            if(candidates == null)
                throw new ArgumentNullException("candidates");
            if(candidates.Count == 0)
                throw new ArgumentException("candidates must not be empty");

            InitMethodMap(candidates);
        }

        /// <summary>
        /// try to resolve the handler method from <paramref name="message"/>
        /// </summary>
        /// <param name="message">the message for which a handler mthod should be resolved</param>
        /// <returns>the handler methodf or <c>null</c> if no appropriate handler method could be found</returns>
        public MethodInfo ResolveHandlerMethod(IMessage message) {
            MethodInfo method = DictionaryUtils.Get(_methodMap, message.GetType());
            if(method == null) {
                Type payloadType = message.Payload.GetType();
                method = DictionaryUtils.Get(_methodMap, payloadType.GetType());
                if(method == null) {
                    method = FindClosestMatch(payloadType);
                }
                if(method == null) {
                    method = _fallbackMethod;
                }
            }
            return method;
        }

        private void InitMethodMap(IEnumerable<MethodInfo> candidates) {
            foreach(MethodInfo method in candidates) {
                Type expectedType = DetermineExpectedType(method);
                if(expectedType == null) {
                    AssertUtils.IsTrue(_fallbackMethod == null,
                            "At most one method can expect only Message headers rather than a Message or payload, " +
                            "but found two: [" + method + "] and [" + _fallbackMethod + "]");
                    _fallbackMethod = method;
                }
                else {
                    AssertUtils.IsTrue(!_methodMap.ContainsKey(expectedType), "More than one method matches type [" + expectedType + "]. Consider using annotations or providing a method name.");
                    _methodMap.Add(expectedType, method);
                }
            }
        }

        private static Type DetermineExpectedType(MethodInfo method) {
            Type expectedType = null;

            foreach(ParameterInfo pi in method.GetParameters()) {
                object[] parameterAnnotations = pi.GetCustomAttributes(false);

                if(!HandlerMethodUtils.ContainsHeaderAnnotation(parameterAnnotations)) {
                    if(expectedType != null)
                        throw new ArgumentException("Message-handling method must only have one parameter expecting a Message or Message payload."
                            + " Other parameters may be included but only if they have @Header or @Headers annotations.");

                    Type parameterType = pi.ParameterType;
                    if(parameterType.IsGenericType) {
                                if (typeof(IMessage).IsAssignableFrom(parameterType)) {
                                    expectedType = DetermineExpectedTypeFromParameterizedMessageType(parameterType);
                                }
                                else {
                                    Type[] genericArguments = parameterType.GetGenericArguments();
                                    if (genericArguments.Length > 1)
                                        throw new ArgumentException("Message-handling method: error on generic parameter [" + pi.ParameterType + " " + pi.Name + "]");

                                    expectedType = genericArguments[0];
                                }
                            }
                    else 
                        expectedType = parameterType;
                }
            }

            return expectedType;
        }

        private MethodInfo FindClosestMatch(Type payloadType) {
            ICollection<Type> expectedTypes = _methodMap.Keys;
            int minTypeDiffWeight = Int32.MaxValue;
            MethodInfo matchingMethod = null;
            foreach(Type expectedType in expectedTypes) {
                int typeDiffWeight = GetTypeDifferenceWeight(expectedType, payloadType);
                if(typeDiffWeight < minTypeDiffWeight) {
                    minTypeDiffWeight = typeDiffWeight;
                    matchingMethod = DictionaryUtils.Get(_methodMap, expectedType);
                }
            }
            if(matchingMethod != null) {
                DictionaryUtils.Put(_methodMap, payloadType, matchingMethod);
            }
            return matchingMethod;
        }

        private static Type DetermineExpectedTypeFromParameterizedMessageType(Type type) {
            Type expectedType = type.GetGenericArguments()[0];
            return expectedType;
        }

        private static int GetTypeDifferenceWeight(Type expectedType, Type payloadType) {
            int result = 0;
            if(!expectedType.IsAssignableFrom(payloadType)) {
                return Int32.MaxValue;
            }

            Type superClass = payloadType.BaseType;
            while(superClass != null) {
                if(expectedType.Equals(superClass)) {
                    result = result + 2;
                    superClass = null;
                }
                else if(expectedType.IsAssignableFrom(superClass)) {
                    result = result + 2;
                    superClass = superClass.BaseType;
                }
                else {
                    superClass = null;
                }
            }
            if(expectedType.IsInterface) {
                result = result + 1;
            }
            return result;
        }
    }
}
