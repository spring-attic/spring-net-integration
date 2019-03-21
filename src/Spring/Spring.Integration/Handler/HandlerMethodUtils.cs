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

namespace Spring.Integration.Handler {
    /// <summary>
    /// Utility methods for common behavior related to Message-handling methods.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class HandlerMethodUtils {

        /// <summary>
        /// Verifies that the provided Method is capable of handling Messages.
        /// It must accept at least one parameter, and if it expects more than one
        /// parameter, at most one of them may expect the payload object. Others
        /// must be annotated for accepting Message header values.
        /// </summary>
        /// <param name="method">the method to test</param>
        /// <returns><c>true</c> if the <paramref name="method"/> is valid, otherwise <c>false</c></returns>
        public static bool IsValidHandlerMethod(MethodInfo method) {
            if(method.DeclaringType.Equals(typeof(object))) {
                return false;
            }
            if(!method.IsPublic) {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if(parameters.Length == 0) {
                return false;
            }

            if(parameters.Length > 1) {
                // at most one parameter can be lacking @Header or @Headers or one parameter can be annotated as payload
                int parameterWithHeaderAttribute = CountParametersWithAttribute(parameters, typeof(HeaderAttribute));

                if(parameterWithHeaderAttribute == 0 || parameterWithHeaderAttribute < parameters.Length - 1)
                    return false;
            }
            return true;
        }

        private static int CountParametersWithAttribute(IEnumerable<ParameterInfo> parameters, Type attributeType) {
            int count = 0;
            foreach(ParameterInfo parameter in parameters) {
                object[] headerAttributes = parameter.GetCustomAttributes(attributeType, false);

                if(headerAttributes.Length >= 1)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// analyzes <paramref name="obj"/> for valid hanlder methods
        /// </summary>
        /// <param name="obj">the object to analyze</param>
        /// <returns>a list of valid handler methods</returns>
        public static IList<MethodInfo> GetCandidateHandlerMethods(object obj) {
            IList<MethodInfo> candidates = new List<MethodInfo>();
            Type type = AopUtils.GetTargetType(obj);
            if(type == null) {
                type = obj.GetType();
            }
            foreach(MethodInfo method in type.GetMethods()) {
                if(IsValidHandlerMethod(method)) {
                    candidates.Add(method);
                }
            }
            return candidates;
        }
 
        /// <summary>
        /// Checks the array of Annotations for a method parameter to see if either
        /// the @Header or @Headers annotation is present.
        /// </summary>
        /// <param name="parameterAnnotations"></param>
        /// <returns></returns>
        public static bool ContainsHeaderAnnotation(object[] parameterAnnotations) {
            foreach(object annotation in parameterAnnotations) {
                if(annotation.GetType().Equals(typeof(HeaderAttribute)) || annotation.GetType().Equals(typeof(HeadersAttribute))) {
                    return true;
                }
            }
            return false;
        }
    }
}