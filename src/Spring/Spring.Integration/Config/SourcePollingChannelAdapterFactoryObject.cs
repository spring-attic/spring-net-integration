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
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Integration.Scheduling;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Util;

namespace Spring.Integration.Config {
    /// <summary>
    /// FactoryBean for creating a SourcePollingChannelAdapter instance.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class SourcePollingChannelAdapterFactoryObject : IFactoryObject, IObjectFactoryAware, IObjectNameAware, /*BeanClassLoaderAware,*/ IInitializingObject {

        private volatile IMessageSource _source;

        private volatile IMessageChannel _outputChannel;

        private volatile PollerMetadata _pollerMetadata;

        private volatile bool _autoStartup = true;

        private volatile string _objectName;

        private volatile IConfigurableObjectFactory _objectFactory;

        //private volatile ClassLoader beanClassLoader;

        private volatile SourcePollingChannelAdapter _adapter;

        private volatile bool _initialized;

        private object _initializationMonitor = new object();


        public IMessageSource Source {
            set { _source = value; }
        }

        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        public PollerMetadata PollerMetadata {
            set { _pollerMetadata = value; }
        }

        public bool AutoStartup {
            set { _autoStartup = value; }
        }

        public IObjectFactory ObjectFactory {
            set {
                AssertUtils.AssertArgumentType(value, "ObjectFactory", typeof(IConfigurableObjectFactory), "a ConfigurableBeanFactory is required");
                _objectFactory = (IConfigurableObjectFactory)value;
            }
        }

        //public void setBeanClassLoader(ClassLoader classLoader) {
        //    this.beanClassLoader = classLoader;
        //}

        public string ObjectName {
            set { _objectName = value; }
        }

        public void AfterPropertiesSet() {
            InitializeAdapter();
        }

        public object GetObject() {
            if(_adapter == null) {
                InitializeAdapter();
            }
            return _adapter;
        }

        public Type ObjectType {
            get { return typeof(SourcePollingChannelAdapter); }
        }

        public bool IsSingleton {
            get { return true; }
        }

        private void InitializeAdapter() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }
                AssertUtils.ArgumentNotNull(_source, "source is required");
                AssertUtils.ArgumentNotNull(_outputChannel, "outputChannel is required");
                SourcePollingChannelAdapter spca = new SourcePollingChannelAdapter();
                spca.Source = _source;
                spca.OutputChannel = _outputChannel;
                if(_pollerMetadata == null) {
                    _pollerMetadata = IntegrationContextUtils.GetDefaultPollerMetadata(_objectFactory);
                    AssertUtils.ArgumentNotNull(_pollerMetadata, "No poller has been defined for channel-adapter '" + _objectName + "', and no default poller is available within the context.");
                }
                spca.Trigger = _pollerMetadata.Trigger;
                spca.MaxMessagesPerPoll = _pollerMetadata.MaxMessagesPerPoll;
                spca.TaskExecutor = _pollerMetadata.TaskExecutor;
                spca.TransactionManager = _pollerMetadata.TransactionManager;
                spca.TransactionDefinition = _pollerMetadata.TransactionDefinition;
                spca.AutoStartup = _autoStartup;
                spca.ObjectName = _objectName;
                spca.ObjectFactory = _objectFactory;
                //spca.setBeanClassLoader(this.beanClassLoader);
                spca.AfterPropertiesSet();
                _adapter = spca;
                _initialized = true;
            }
        }
    }
}
