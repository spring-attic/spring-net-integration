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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// A base or helper class for any Messaging component that acts as an adapter
    /// by invoking a "plain" (not Message-aware) method on a given target object.
    /// The target Object is mandatory, and either a {@link Method} reference, a
    /// 'methodName', or an Annotation type must be provided.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageMappingMethodInvoker {

        protected static ILog logger = LogManager.GetLogger(typeof(MessageMappingMethodInvoker));

        // TODO Set<>
        private readonly IList<MethodInfo> methodsExpectingMessage = new List<MethodInfo>();

        private volatile object _obj;

        private readonly IHandlerMethodResolver _methodResolver;

        private readonly IDictionary<MethodInfo, IOutboundMessageMapper> messageMappers = new Dictionary<MethodInfo, IOutboundMessageMapper>();

        private readonly IDictionary<MethodInfo, IMethodInvoker> invokers = new Dictionary<MethodInfo, IMethodInvoker>();


        public MessageMappingMethodInvoker(object obj, MethodInfo method) {
            AssertUtils.ArgumentNotNull(obj, "object must not be null");
            AssertUtils.ArgumentNotNull(method, "method must not be null");
            _obj = obj;
            _methodResolver = new StaticHandlerMethodResolver(method);
        }

        public MessageMappingMethodInvoker(object obj, Type attributeType) {
            AssertUtils.ArgumentNotNull(obj, "object must not be null");
            AssertUtils.ArgumentNotNull(attributeType, "annotation type must not be null");
            _obj = obj;
            _methodResolver = CreateResolverForAnnotation(attributeType);
        }

        public MessageMappingMethodInvoker(object obj, string methodName) {
            AssertUtils.ArgumentNotNull(obj, "object must not be null");
            AssertUtils.ArgumentNotNull(methodName, "methodName must not be null");
            _obj = obj;
            _methodResolver = CreateResolverForMethodName(methodName);
        }

        public object InvokeMethod(IMessage message) {
            AssertUtils.ArgumentNotNull(message, "message must not be null");
            if(message.Payload == null) {
                if(logger.IsDebugEnabled) {
                    logger.Debug("received null payload");
                }
                return null;
            }
            MethodInfo method = _methodResolver.ResolveHandlerMethod(message);
            object[] args = CreateArgumentArrayFromMessage(method, message);
            try {
                return DoInvokeMethod(method, args, message);
            }
            catch(TargetInvocationException e) {
                
                if(e.InnerException != null && e.InnerException is SystemException) {
                    //MLP    throw e.InnerException;
                    throw ReflectionUtils.UnwrapTargetInvocationException(e);
                }
                throw new MessageHandlingException(message, "method '" + method + "' threw an Exception.", e.InnerException);
                

            }
            catch(Exception e) {
                if(e is SystemException) {
                    throw;
                }
                throw new MessageHandlingException(message, "Failed to invoke method '" + method + "' with arguments: " + (args != null ? args.ToString() : "<null>"), e);
            }
        }

        private object DoInvokeMethod(MethodInfo method, object[] args, IMessage message) {
            object result = null;
            IMethodInvoker invoker = null;
            try {
                invoker = invokers.ContainsKey(method) ? invokers[method] : null;
                if(invoker == null) {
                    invoker = new DefaultMethodInvoker(_obj, method);
                    invokers.Add(method, invoker);
                }
                result = invoker.InvokeMethod(args);
            }
            catch(ArgumentException e) {
                try {
                    if(message != null) {
                        result = invoker.InvokeMethod(message);
                        methodsExpectingMessage.Add(method);
                    }
                }
                catch(Exception ex) { //TODO NoSuchMethodException e2) {
                    throw new MessageHandlingException(message, "unable to resolve method for args: " + StringUtils.ArrayToCommaDelimitedString(args), ex);
                }
            }
            return result == null || result.GetType().Equals(typeof(Missing)) ? null : result;
        }

        private object[] CreateArgumentArrayFromMessage(MethodInfo method, IMessage message) {
            object[] args = null;
            object mappingResult = methodsExpectingMessage.Contains(method) ? message : ResolveParameters(method, message);
            if(mappingResult != null && mappingResult.GetType().IsArray
                    && typeof(object).IsAssignableFrom(mappingResult.GetType().GetElementType())) {
                args = (object[])mappingResult;
            }
            else {
                args = new[] { mappingResult };
            }
            return args;
        }

        private object ResolveParameters(MethodInfo method, IMessage message) {
            IOutboundMessageMapper mapper = messageMappers.ContainsKey(method) ? messageMappers[method] : null;
            if(mapper == null) {
                mapper = new MethodParameterMessageMapper(method);
                messageMappers.Add(method, mapper);
            }
            // TODO check
            return mapper.FromMessage(message);
        }

        private IHandlerMethodResolver CreateResolverForMethodName(string methodName) {
            IList<MethodInfo> methodsWithName = new List<MethodInfo>();
            foreach(MethodInfo method in HandlerMethodUtils.GetCandidateHandlerMethods(_obj)) {
                if(method.Name.Equals(methodName)) {
                    methodsWithName.Add(method);
                }
            }
            if(methodsWithName.Count == 0)
                throw new ArgumentException("Failed to find any valid Message-handling methods named '" + methodName + "' on target class [" + _obj.GetType() + "].");
            if(methodsWithName.Count == 1) {
                return new StaticHandlerMethodResolver(methodsWithName[0]);
            }

            return new PayloadTypeMatchingHandlerMethodResolver(methodsWithName);
        }

        private IHandlerMethodResolver CreateResolverForAnnotation(Type attributeType) {
            IList<MethodInfo> methodsWithAnnotation = new List<MethodInfo>();
            IList<MethodInfo> defaultCandidateMethods = HandlerMethodUtils.GetCandidateHandlerMethods(_obj);
            foreach(MethodInfo method in defaultCandidateMethods) {
                object[] annotations = method.GetCustomAttributes(attributeType, true);
                if(annotations != null && annotations.Length > 0) {
                    methodsWithAnnotation.Add(method);
                }
            }
            IList<MethodInfo> candidateMethods = (methodsWithAnnotation.Count == 0) ? null : methodsWithAnnotation;
            if(candidateMethods == null) {
                if(logger.IsInfoEnabled) {
                    logger.Info("Failed to find any valid Message-handling methods with annotation ["
                            + attributeType + "] on target class [" + _obj.GetType() + "]. "
                            + "Method-resolution will be applied to all eligible methods.");
                }
                candidateMethods = defaultCandidateMethods;
            }
            if(candidateMethods.Count == 1) {
                return new StaticHandlerMethodResolver(candidateMethods[0]);
            }
            return new PayloadTypeMatchingHandlerMethodResolver(candidateMethods);
        }
    }
}
