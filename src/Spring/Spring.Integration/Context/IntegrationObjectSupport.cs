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

using Common.Logging;
using Spring.Integration.Channel;
using Spring.Integration.Scheduling;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Context {
    /// <summary>
    /// A base class that provides convenient access to the bean factory as
    /// well as {@link ChannelResolver} and {@link TaskScheduler} instances.
    /// 
    /// <p>This is intended to be used as a base class for internal framework
    /// components whereas code built upon the integration framework should not
    /// require tight coupling with the context but rather rely on standard
    /// dependency injection.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class IntegrationObjectSupport : IObjectNameAware, IObjectFactoryAware {

        /** Logger that is available to subclasses */
        protected ILog logger = LogManager.GetLogger(typeof(IntegrationObjectSupport));

        private volatile string _objectName;

        private volatile IObjectFactory _objectFactory;

        private volatile IChannelResolver _channelResolver;

        private volatile ITaskScheduler _taskScheduler;

        public string ObjectName {
            get { return _objectName; }
            set { _objectName = value; }
        }

        public IObjectFactory ObjectFactory {
            get { return _objectFactory; }
            set {
                AssertUtils.ArgumentNotNull(value, "ObjectFactory must not be null");
                _objectFactory = value;
                _channelResolver = new ObjectFactoryChannelResolver(_objectFactory);
                ITaskScheduler taskScheduler = IntegrationContextUtils.GetTaskScheduler(_objectFactory);
                if(taskScheduler != null) {
                    _taskScheduler = taskScheduler;
                }
            }
        }

        protected IChannelResolver ChannelResolver {
            get { return _channelResolver; }
        }

        public ITaskScheduler TaskScheduler {
            protected get { return _taskScheduler; }
            set {
                AssertUtils.ArgumentNotNull(value, "taskScheduler must not be null");
                _taskScheduler = value;
            }
        }

        public override string ToString() {
            return _objectName != null ? _objectName : base.ToString();
        }
    }
}
