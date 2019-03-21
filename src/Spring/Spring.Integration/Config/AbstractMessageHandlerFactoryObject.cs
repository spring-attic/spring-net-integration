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
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Config {
    /// <summary>
    /// Base class for FactoryBeans that create MessageHandler instances. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractMessageHandlerFactoryObject : IFactoryObject {

        private volatile IMessageHandler _handler;

        private volatile object _targetObject;

        private volatile string _targetMethodName;

        private volatile IMessageChannel _outputChannel;

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();


        public object TargetObject {
            set { _targetObject = value; }
        }

        public string TargetMethodName {
            set { _targetMethodName = value; }
        }

        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        public object GetObject() {
            if(_handler == null) {
                InitializeHandler();
                AssertUtils.ArgumentNotNull(_handler, "failed to create MessageHandler");
                if(_outputChannel != null
                        && _handler is AbstractReplyProducingMessageHandler) {
                    ((AbstractReplyProducingMessageHandler)_handler).OutputChannel = _outputChannel;
                }
            }
            return _handler;
        }

        public Type ObjectType {
            get {
                if(_handler != null) {
                    return _handler.GetType();
                }
                return typeof(IMessageHandler);
                ;
            }
        }

        public bool IsSingleton {
            get { return true; }
        }

        private void InitializeHandler() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }
                _handler = CreateHandler(_targetObject, _targetMethodName);
                _initialized = true;
            }
        }

        /**
         * Subclasses must implement this method to create the MessageHandler.
         */
        protected abstract IMessageHandler CreateHandler(object targetObject, string targetMethodName);
    }
}
