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
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Integration.Scheduling;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Integration.Config {

    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class ConsumerEndpointFactoryObject : IFactoryObject, IObjectFactoryAware, IObjectNameAware, IInitializingObject, IApplicationEventListener {

        private readonly IMessageHandler _handler;

        private volatile string _objectName;

        private volatile string _inputChannelName;

        private volatile PollerMetadata _pollerMetadata;

        private volatile bool _autoStartup = true;

        private volatile IConfigurableObjectFactory _objectFactory;

        private volatile AbstractEndpoint _endpoint;

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();

        /// <summary>
        /// create a new <see cref="ConsumerEndpointFactoryObject"/>
        /// </summary>
        /// <param name="handler">the <see cref="IMessageHandler"/></param>
        public ConsumerEndpointFactoryObject(IMessageHandler handler) {
            AssertUtils.ArgumentNotNull(handler, "handler must not be null");
            _handler = handler;
        }

        /// <summary>
        /// set the input channel
        /// </summary>
        public string InputChannelName {
            set { _inputChannelName = value; }
        }

        /// <summary>
        /// set the poller metadata
        /// </summary>
        public PollerMetadata PollerMetadata {
            set { _pollerMetadata = value; }
        }

        /// <summary>
        /// set auto startup
        /// </summary>
        public bool AutoStartup {
            set { _autoStartup = value; }
        }

        public string ObjectName {
            set { _objectName = value; }
        }

        public IObjectFactory ObjectFactory {
            set {
                _objectFactory = value as IConfigurableObjectFactory;
                if(_objectFactory == null)
                    throw new ArgumentException("a ConfigurableObjectFactory is required");
            }
        }

        public void AfterPropertiesSet() {
            InitializeEndpoint();
        }

        public bool IsSingleton {
            get { return true; }
        }

        public object GetObject() {
            if(!_initialized) {
                InitializeEndpoint();
            }
            return _endpoint;
        }

        public Type ObjectType {
            get {
                if (_endpoint == null) {
                    return typeof (AbstractEndpoint);
                }
                return _endpoint.GetType();
            }
        }

        private void InitializeEndpoint() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }

                AssertUtils.ArgumentHasText(_inputChannelName, "inputChannelName is required");

                AssertUtils.IsTrue(_objectFactory.ContainsObject(_inputChannelName), "no such input channel '" + _inputChannelName + "' for endpoint '" + _objectName + "'");

                IMessageChannel channel = (IMessageChannel)_objectFactory.GetObject(_inputChannelName, typeof(IMessageChannel));
                if(channel is ISubscribableChannel) {
                    if (_pollerMetadata != null)
                        throw new ArgumentException("A poller should not be specified for endpoint '" + _objectName
                                                                + "', since '" + _inputChannelName + "' is a SubscribableChannel (not pollable).");
                    _endpoint = new EventDrivenConsumer((ISubscribableChannel)channel, _handler);
                }
                else if(channel is IPollableChannel) {
                    PollingConsumer pollingConsumer = new PollingConsumer((IPollableChannel)channel, _handler);
                    if(_pollerMetadata == null) {
                        _pollerMetadata = IntegrationContextUtils.GetDefaultPollerMetadata(_objectFactory);
                        AssertUtils.ArgumentNotNull(_pollerMetadata, "No poller has been defined for endpoint '" + _objectName + "', and no default poller is available within the context.");
                    }
                    pollingConsumer.Trigger = _pollerMetadata.Trigger;
                    pollingConsumer.MaxMessagesPerPoll = _pollerMetadata.MaxMessagesPerPoll;
                    pollingConsumer.ReceiveTimeout = _pollerMetadata.ReceiveTimeout;
                    pollingConsumer.TaskExecutor = _pollerMetadata.TaskExecutor;
                    pollingConsumer.TransactionManager = _pollerMetadata.TransactionManager;
                    pollingConsumer.TransactionDefinition = _pollerMetadata.TransactionDefinition;
                    pollingConsumer.AdviceChain = _pollerMetadata.AdviceChain;
                    _endpoint = pollingConsumer;
                }
                else {
                    throw new ArgumentException("unsupported channel type: [" + channel.GetType() + "]");
                }
                _endpoint.AutoStartup = _autoStartup;
                _endpoint.ObjectName = _objectName;
                _endpoint.ObjectFactory = _objectFactory;
                _endpoint.AfterPropertiesSet();
                _initialized = true;
            }
        }

        #region IApplicationEventListener Members

        public void HandleApplicationEvent(object sender, ApplicationEventArgs e) {
            if (_endpoint is IApplicationEventListener) {
                ((IApplicationEventListener) _endpoint).HandleApplicationEvent(sender, e);
            }
        }

        #endregion
    }
}
