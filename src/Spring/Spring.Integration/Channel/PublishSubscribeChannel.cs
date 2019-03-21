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

using Spring.Integration.Dispatcher;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Util;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A channel that sends Messages to each of its subscribers. 
    /// queue.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class PublishSubscribeChannel : AbstractSubscribableChannel<BroadcastingDispatcher>, IObjectFactoryAware {

        private volatile IExecutor _taskExecutor;
        private volatile IErrorHandler _errorHandler;

        /// <summary>
        /// Create a PublishSubscribeChannel that will use a {@link TaskExecutor}
        /// to invoke the handlers. If this is null, each invocation will occur in the message sender's thread. 
        /// </summary>
        /// <param name="taskExecutor">the task executor to publish the messages</param>
        public PublishSubscribeChannel(IExecutor taskExecutor)
            : base(new BroadcastingDispatcher(taskExecutor)) {
            _taskExecutor = taskExecutor;
        }

        /// <summary>
        /// Create a PublishSubscribeChannel that will invoke the handlers in the  message sender's thread. 
        /// </summary>
        public PublishSubscribeChannel() : this(null) { }

         /// <summary>
        /// set the error handler
        /// </summary>
        public IErrorHandler ErrorHandler {
            set { _errorHandler = value; }
        }

        //TODO add IgnoreFailures property

        /// <summary>
        /// Specify whether to apply the sequence number and size headers to the
        /// messages prior to invoking the subscribed handlers. 
        /// </summary>
        /// <remarks>
        /// By default, this value is false meaning that sequence headers will
        /// not be applied. If planning to use an Aggregator downstream
        /// with the default correlation and completion strategies, you should set
        /// this flag to true.
        /// </remarks>
        public bool ApplySequence {
            set { Dispatcher.ApplySequence = value; }
        }

        #region IObjectFactoryAware Members

        public IObjectFactory ObjectFactory {
            set {
                if (_taskExecutor != null) {
                    ErrorHandlingTaskExecutor errorHandlingTaskExecutor = _taskExecutor as ErrorHandlingTaskExecutor;
                    if (errorHandlingTaskExecutor == null)
                    {
                        if (_errorHandler == null) {
                            _errorHandler = new MessagePublishingErrorHandler(new ObjectFactoryChannelResolver(value));
                        }
                        _taskExecutor = new ErrorHandlingTaskExecutor(_taskExecutor, _errorHandler);
                    }
                    Dispatcher.TaskExecutor = _taskExecutor;
                }
            }
        }

        #endregion
    }
}
