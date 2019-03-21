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
using System.ComponentModel;
using System.Reflection;
using AopAlliance.Intercept;
using Spring.Aop.Framework;
using Spring.Context;
using Spring.Core.TypeConversion;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Gateway {

    /// <summary>
    /// Generates a proxy for the provided service interface to enable interaction
    /// with messaging components without application code being aware of them.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class GatewayProxyFactoryObject : AbstractEndpoint, IFactoryObject, IMethodInterceptor { //}, BeanClassLoaderAware {

        private volatile Type _serviceInterface;

        private volatile IMessageChannel _defaultRequestChannel;

        private volatile IMessageChannel _defaultReplyChannel;

        private /*volatile*/ TimeSpan _defaultRequestTimeout = TimeSpan.FromMilliseconds(-1);

        private /*volatile*/ TimeSpan _defaultReplyTimeout = TimeSpan.FromMilliseconds(-1);

        //private volatile TypeConverter _typeConverter = new SimpleTypeConverter();

        //private volatile ClassLoader beanClassLoader = ClassUtils.getDefaultClassLoader();

        private volatile object _serviceProxy;

        private readonly IDictionary<MethodInfo, IMessagingGateway> _gatewayMap = new Dictionary<MethodInfo, IMessagingGateway>();

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();


        public Type ServiceInterface {
            set {
                if(value != null && !value.IsInterface) {
                    throw new ArgumentException("'serviceInterface' must be an interface");
                }
                _serviceInterface = value;
            }
        }

        /// <summary>
        /// Set the default request channel to which request messages will
        /// be sent if no request channel has been configured with an annotation
        /// </summary>
        public IMessageChannel DefaultRequestChannel {
            set { _defaultRequestChannel = value; }
        }

        /// <summary>
        /// Set the default reply channelfrom which reply messages will be
        /// received if no reply channel has been configured with an annotation.
        /// If no default reply channel is provided,
        /// and no reply channel is configured with annotations, an anonymous,
        /// temporary channel will be used for handling replies.
        /// </summary>
        public IMessageChannel DefaultReplyChannel {
            set { _defaultReplyChannel = value; }
        }

        /// <summary>
        /// Set the default timeout value for sending request messages. If not
        /// explicitly configured with an annotation, this value will be used.
        /// </summary>
        public TimeSpan DefaultRequestTimeout {
            set {
                lock(this) {
                    _defaultRequestTimeout = value;
                }
            }
        }

        /// <summary>
        /// Set the default timeout value for receiving reply messages. If not
        /// explicitly configured with an annotation, this value will be used.
        /// </summary>
        public TimeSpan DefaultReplyTimeout {
            set {
                lock(this) {
                    _defaultReplyTimeout = value;
                }
            }
        }

        //public TypeConverter TypeConverter {
        //    set {
        //        AssertUtils.ArgumentNotNull(value, "typeConverter must not be null");
        //        _typeConverter = value;
        //    }
        //}

        //public void setBeanClassLoader(ClassLoader beanClassLoader) {
        //    this.beanClassLoader = beanClassLoader;
        //}

        protected override void OnInit() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }
                if(_serviceInterface == null) {
                    throw new ArgumentException("'serviceInterface' must not be null");
                }
                MethodInfo[] methods = _serviceInterface.GetMethods();
                foreach(MethodInfo method in methods) {
                    IMessagingGateway gateway = CreateGatewayForMethod(method);
                    _gatewayMap.Add(method, gateway);
                }

                ProxyFactory pf = new ProxyFactory(new[] {_serviceInterface});
                pf.AddAdvice(this);
                _serviceProxy = pf.GetProxy();
                Start();
                _initialized = true;
            }
        }

        public object GetObject() {
            return _serviceProxy;
        }

        public Type ObjectType {
            get { return _serviceInterface; }
        }

        public bool IsSingleton {
            get { return true; }
        }

        public object Invoke(IMethodInvocation invocation) {
            MethodInfo method = invocation.Method;
            if(method.Name.Equals("ToString")) { // AopUtils.isToStringMethod(method)) {
                return "gateway proxy for service interface [" + _serviceInterface + "]";
            }
            if(method.DeclaringType.Equals(_serviceInterface)) {
                return InvokeGatewayMethod(invocation);
            }
            return invocation.Proceed();
        }

        private object InvokeGatewayMethod(IMethodInvocation invocation) {
            if(!_initialized) {
                AfterPropertiesSet();
            }
            MethodInfo method = invocation.Method;
            IMessagingGateway gateway = _gatewayMap[method];
            Type returnType = method.ReturnType;
            bool isReturnTypeMessage = typeof(IMessage).IsAssignableFrom(returnType);
            bool shouldReply = returnType != typeof(void);
            int paramCount = method.GetParameters().Length;
            object response = null;
            if(paramCount == 0) {
                if(shouldReply) {
                    if(isReturnTypeMessage) {
                        return gateway.Receive();
                    }
                    response = gateway.Receive();
                }
            }
            else {
                object[] args = invocation.Arguments;
                if(shouldReply) {
                    response = isReturnTypeMessage ? gateway.SendAndReceiveMessage(args) : gateway.SendAndReceive(args);
                }
                else {
                    gateway.Send(args);
                    response = null;
                }
            }
            return (response != null) ? TypeConversionUtils.ConvertValueIfNecessary(returnType, response, null) : null;
        }

        private IMessagingGateway CreateGatewayForMethod(MethodInfo method) {
            SimpleMessagingGateway gateway = new SimpleMessagingGateway(new MethodParameterMessageMapper(method), new SimpleMessageMapper());
            if(TaskScheduler != null) {
                gateway.TaskScheduler = TaskScheduler;
            }
            GatewayAttribute gatewayAttribute = null;
            object[] attr = method.GetCustomAttributes(typeof(GatewayAttribute), false);
            if(attr.Length == 1)
                gatewayAttribute = attr[0] as GatewayAttribute;

            IMessageChannel requestChannel = _defaultRequestChannel;
            IMessageChannel replyChannel = _defaultReplyChannel;
            TimeSpan requestTimeout = _defaultRequestTimeout;
            TimeSpan replyTimeout = _defaultReplyTimeout;
            if(gatewayAttribute != null) {
                AssertUtils.State(ChannelResolver != null, "ChannelResolver is required");
                string requestChannelName = gatewayAttribute.RequestChannel;
                if(StringUtils.HasText(requestChannelName)) {
                    if(ChannelResolver == null)
                        throw new InvalidOperationException("ChannelResolvr must not be null");

                    requestChannel = ChannelResolver.ResolveChannelName(requestChannelName);

                    if(requestChannel == null)
                        throw new InvalidAsynchronousStateException("failed to resolve request channel '" + requestChannelName + "'");
                }

                string replyChannelName = gatewayAttribute.ReplyChannel;
                if(StringUtils.HasText(replyChannelName)) {
                    if(ChannelResolver == null)
                        throw new InvalidOperationException("ChannelResolvr must not be null");

                    replyChannel = ChannelResolver.ResolveChannelName(replyChannelName);

                    if(replyChannel == null)
                        throw new InvalidAsynchronousStateException("failed to resolve reply channel '" + replyChannelName + "'");
                }
                requestTimeout = gatewayAttribute.RequestTimeout;
                replyTimeout = gatewayAttribute.ReplyTimeout;
            }
            gateway.RequestChannel = requestChannel;
            gateway.ReplyChannel = replyChannel;
            gateway.RequestTimeout = requestTimeout;
            gateway.ReplyTimeout = replyTimeout;
            if(ObjectFactory != null) {
                gateway.ObjectFactory = ObjectFactory;
            }
            return gateway;
        }

        // Lifecycle implementation

        // guarded by super#lifecycleLock
        protected override void DoStart() {
            foreach(IMessagingGateway gateway in _gatewayMap.Values) {
                if(gateway is ILifecycle) {
                    ((ILifecycle)gateway).Start();
                }
            }
        }

        // guarded by super#lifecycleLock
        protected override void DoStop() {
            foreach(IMessagingGateway gateway in _gatewayMap.Values) {
                if(gateway is ILifecycle) {
                    ((ILifecycle)gateway).Stop();
                }
            }
        }
    }
}
