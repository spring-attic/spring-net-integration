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
using AopAlliance.Aop;
using Spring.Aop.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Scheduling;
using Spring.Integration.Util;
using Spring.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Integration.Endpoint {
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractPollingEndpoint : AbstractEndpoint { 

        public static int MAX_MESSAGES_UNBOUNDED = -1;


        private volatile ITrigger _trigger;

        protected long maxMessagesPerPoll = MAX_MESSAGES_UNBOUNDED;

        private volatile IExecutor _taskExecutor;

        private volatile IErrorHandler _errorHandler;

        private volatile IPlatformTransactionManager _transactionManager;

        private volatile ITransactionDefinition _transactionDefinition;

        private volatile TransactionTemplate _transactionTemplate;

        private readonly IList<IAdvice> _adviceChain = new CopyOnWriteArrayList<IAdvice>();

        private volatile IScheduledFuture<object> _runningTask;

        private volatile IRunnable _poller;

        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();


        /// <summary>
        /// set the trigger 
        /// </summary>
        public ITrigger Trigger {
            set { _trigger = value; }
        }

        /// <summary>
        /// Set the maximum number of messages to receive for each poll.
        /// A non-positive value indicates that polling should repeat as long
        /// as non-null messages are being received and successfully sent.
        /// 
        /// <p>The default is unbounded.
        ///  
        /// <see cref="MAX_MESSAGES_UNBOUNDED"/>
        /// </summary>
        public int MaxMessagesPerPoll {
            set {
                lock (this) {
                    maxMessagesPerPoll = value;
                }
            }
        }

        /// <summary>
        /// set the task executor
        /// </summary>
        public IExecutor TaskExecutor {
            set { _taskExecutor = value; }
        }

        /// <summary>
        /// set the error handler
        /// </summary>
        public IErrorHandler ErrorHandler {
            set { _errorHandler = value; }
        }

        /// <summary>
        /// Specify a transaction manager to use for all polling operations.
        /// If none is provided, then the operations will occur without any
        /// transactional behavior (i.e. there is no default transaction manager).
        /// </summary>
        public IPlatformTransactionManager TransactionManager {
            set { _transactionManager = value; }
        }

        /// <summary>
        /// set the transaction definition
        /// </summary>
        public ITransactionDefinition TransactionDefinition {
            set { _transactionDefinition = value; }
        }

        /// <summary>
        /// set the advice chain
        /// </summary>
        public IList<IAdvice> AdviceChain {
            set {
                lock(_adviceChain) {
                    _adviceChain.Clear();
                    if(value != null) {
                        foreach(IAdvice advice in value)
                            _adviceChain.Add(advice);
                    }
                }
            }
        }

        private TransactionTemplate TransactionTemplate {
            get {
                if(!_initialized) {
                    OnInit();
                }
                return _transactionTemplate;
            }
        }

        protected override void OnInit() {
            lock(_initializationMonitor) {
                if(_initialized) {
                    return;
                }
                AssertUtils.ArgumentNotNull(_trigger, "trigger is required");
                if(_transactionManager != null) {
                    if(_transactionDefinition == null) {
                        _transactionDefinition = new DefaultTransactionDefinition();
                    }
                    _transactionTemplate = new TransactionTemplate(_transactionManager); //, this.transactionDefinition);
                }
                if(_taskExecutor != null && !(_taskExecutor is ErrorHandlingTaskExecutor)) {
                    if(_errorHandler == null) {
                        _errorHandler = new MessagePublishingErrorHandler(new ObjectFactoryChannelResolver(ObjectFactory));
                    }
                    _taskExecutor = new ErrorHandlingTaskExecutor(_taskExecutor, _errorHandler);
                }
                _poller = CreatePoller();
                _initialized = true;
            }
        }

        private IRunnable CreatePoller() {
            if(_adviceChain.Count == 0) {
                return new Poller(this);
            }
            ProxyFactory proxyFactory = new ProxyFactory(new Poller(this));
            foreach(IAdvice advice in _adviceChain) {
                proxyFactory.AddAdvice(advice);
            }
            return (IRunnable)proxyFactory.GetProxy();
        }


        // LifecycleSupport implementation

        // guarded by super#lifecycleLock
        protected override void DoStart() {
            if(!_initialized) {
                OnInit();
            }
            if(TaskScheduler == null)
                throw new InvalidOperationException("unable to start polling, no taskScheduler available");

            _runningTask = TaskScheduler.Schedule(_poller, _trigger);
        }

        // guarded by super#lifecycleLock
        protected override void DoStop() {
            if(_runningTask != null) {
                _runningTask.Cancel(true);
            }
            _runningTask = null;
        }


        protected abstract bool DoPoll();


        private class Poller : IRunnable {
            private readonly AbstractPollingEndpoint _outer;

            public Poller(AbstractPollingEndpoint outer) {
                _outer = outer;
            }

            public void Run() {
                if(_outer._taskExecutor != null) {
                    _outer._taskExecutor.Execute(Poll);
                }
                else {
                    Poll();
                }
            }

            private void Poll() {
                int count = 0;
                while(_outer.maxMessagesPerPoll <= 0 || count < _outer.maxMessagesPerPoll) {
                    if(!InnerPoll()) {
                        break;
                    }
                    count++;
                }
            }

            private bool InnerPoll() {
                TransactionTemplate txTemplate = _outer.TransactionTemplate;
                if(txTemplate != null) {
                    return (bool)txTemplate.Execute(DoInTransaction);
                }
                return _outer.DoPoll();
            }

            private object DoInTransaction(ITransactionStatus status) {
                return _outer.DoPoll();
            }

        }
    }
}
