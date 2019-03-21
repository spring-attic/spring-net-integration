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
using Common.Logging;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;
using Spring.Util;

namespace Spring.Integration.Util {

    /// <summary>
    /// JavaBean that allows for configuring a JDK 1.5 {@link java.util.concurrent.ThreadPoolExecutor}
    /// in bean style (through its "corePoolSize", "maxPoolSize", "keepAliveTime", "queueCapacity"
    /// properties), exposing it as a Spring {@link org.springframework.core.task.TaskExecutor}.
    /// This is an alternative to configuring a ThreadPoolExecutor instance directly using
    /// constructor injection, with a separate {@link ConcurrentTaskExecutor} adapter wrapping it.
    /// 
    /// <p>For any custom needs, in particular for defining a
    /// {@link java.util.concurrent.ScheduledThreadPoolExecutor}, it is recommended to
    /// use a straight definition of the Executor instance or a factory method definition
    /// that points to the JDK 1.5 {@link java.util.concurrent.Executors} class.
    /// To expose such a raw Executor as a Spring {@link org.springframework.core.task.TaskExecutor},
    /// simply wrap it with a {@link ConcurrentTaskExecutor} adapter.
    /// 
    /// <p><b>NOTE:</b> This class implements Spring's
    /// {@link org.springframework.core.task.TaskExecutor} interface as well as the JDK 1.5
    /// {@link java.util.concurrent.Executor} interface, with the former being the primary
    /// interface, the other just serving as secondary convenience. For this reason, the
    /// exception handling follows the TaskExecutor contract rather than the Executor contract,
    /// in particular regarding the {@link org.springframework.core.task.TaskRejectedException}.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Andreas D�hring (.NET)</author>
    /// <author>Anindya Chatterjee (.NET)</author>
    public class ThreadPoolTaskExecutor : CustomizableThreadFactory, /*SchedulingTaskExecutor,*/ IExecutor, IObjectNameAware, IInitializingObject, IDisposable {

        protected ILog logger = LogManager.GetLogger(typeof(ThreadPoolTaskExecutor));

        private readonly object _poolSizeMonitor = new Object();

        private int _corePoolSize = 1;

        private int _maxPoolSize = Int32.MaxValue;

        private TimeSpan _keepAliveTime = new TimeSpan(0, 0, 0, 60);

        private bool _allowCoreThreadTimeOut;

        private int _queueCapacity = Int32.MaxValue;

        private IThreadFactory _threadFactory;

        private IRejectedExecutionHandler _rejectedExecutionHandler = new ThreadPoolExecutor.AbortPolicy();

        private bool _waitForTasksToCompleteOnShutdown;

        private bool _threadNamePrefixSet;

        private string _objectName;

        private ThreadPoolExecutor _threadPoolExecutor;


        /// <summary>
        /// create a new ThreadPoolTaskExecutor
        /// </summary>
        public ThreadPoolTaskExecutor() {
            _threadFactory = this;
        }

        /// <summary>
        /// get/et the ThreadPoolExecutor's core pool size. Default is 1.
        /// <p><b>This setting can be modified at runtime.</b></p>
        /// </summary>
        public int CorePoolSize {
            get {
                lock(_poolSizeMonitor) {
                    return _corePoolSize;
                }
            }
            set {
                lock(_poolSizeMonitor) {
                    _corePoolSize = value;
                    if(_threadPoolExecutor != null) {
                        _threadPoolExecutor.CorePoolSize = value;
                    }
                }
            }
        }

        /// <summary>
        /// get/set the ThreadPoolExecutor's maximum pool size.
        /// Default is <code>Int32.MaxValue</code>.
        /// <p><b>This setting can be modified at runtime.</b></p>
        /// </summary>
        public int MaxPoolSize {
            get {
                lock(_poolSizeMonitor) {
                    return _maxPoolSize;
                }
            }
            set {
                lock(_poolSizeMonitor) {
                    _maxPoolSize = value;
                    if(_threadPoolExecutor != null) {
                        _threadPoolExecutor.MaximumPoolSize = value;
                    }
                }
            }
        }

        /// <summary>
        /// get/set the ThreadPoolExecutor's keep-alive seconds. Default is 60 seconds.
        /// <p><b>This setting can be modified at runtime.</b></p>
        /// </summary>
        public TimeSpan KeepAliveTime {
            get {
                lock(_poolSizeMonitor) {
                    return _keepAliveTime;
                }
            }
            set {
                lock(_poolSizeMonitor) {
                    _keepAliveTime = value;
                    if(_threadPoolExecutor != null) {
                        _threadPoolExecutor.KeepAliveTime = value;
                    }
                }
            }
        }

        /// <summary>
        /// Specify whether to allow core threads to time out. This enables dynamic
        /// growing and shrinking even in combination with a non-zero queue (since
        /// the max pool size will only grow once the queue is full).
        /// <p>Default is "false". Note that this feature is only available on Java 6
        /// or above. On Java 5, consider switching to the backport-concurrent
        /// version of ThreadPoolTaskExecutor which also supports this feature.</p>
        /// </summary>
        public bool AllowCoreThreadToTimeOut {
            set { _allowCoreThreadTimeOut = value; }
        }

        /// <summary>
        /// Set the capacity for the ThreadPoolExecutor's BlockingQueue.
        /// Default is <code>Integer.MAX_VALUE</code>.
        /// <p>Any positive value will lead to a LinkedBlockingQueue instance;
        /// any other value will lead to a SynchronousQueue instance.
        /// @see java.util.concurrent.LinkedBlockingQueue</p>
        /// </summary>
        public int QueueCapacity {
            set { _queueCapacity = value; }
        }

        /// <summary>
        /// Set the ThreadFactory to use for the ThreadPoolExecutor's thread pool.
        /// <p>Default is this executor itself (i.e. the factory that this executor
        /// inherits from). See {@link org.springframework.util.CustomizableThreadCreator}'s
        /// javadoc for available bean properties.</p>
        /// </summary>
        public IThreadFactory ThreadFactory {
            set { _threadFactory = (value ?? this); }
        }

        /// <summary>
        /// Set the RejectedExecutionHandler to use for the ThreadPoolExecutor.
        /// Default is the ThreadPoolExecutor's default abort policy.
        /// </summary>
        public IRejectedExecutionHandler RejectedExecutionHandler {
            set {
                _rejectedExecutionHandler = (value ?? new ThreadPoolExecutor.AbortPolicy());
            }
        }

        /// <summary>
        /// Set whether to wait for scheduled tasks to complete on shutdown.
        /// <p>Default is "false". Switch this to "true" if you prefer
        /// fully completed tasks at the expense of a longer shutdown phase.</p>
        /// </summary>
        public bool WaitForTasksToCompleteOnShutdown {
            set { _waitForTasksToCompleteOnShutdown = value; }
        }

        /// <summary>
        /// set the prefix for thread names
        /// </summary>
        public new string ThreadNamePrefix {
            set {
                base.ThreadNamePrefix = value;
                _threadNamePrefixSet = true;
            }
        }

        public string ObjectName {
            set { _objectName = value; }
        }

        /// <summary>
        /// Calls <code>Initialize()</code> after the container applied all property values.
        /// </summary>
        public void AfterPropertiesSet() {
            Initialize();
        }

        /// <summary>
        /// Creates the BlockingQueue and the ThreadPoolExecutor.
        /// </summary>
        public void Initialize() {
            if(logger.IsInfoEnabled) {
                logger.Info("Initializing ThreadPoolExecutor" + (_objectName != null ? " '" + _objectName + "'" : ""));
            }
            if(!_threadNamePrefixSet && _objectName != null) {
                ThreadNamePrefix = _objectName + "-";
            }
            IBlockingQueue<IRunnable> queue = CreateQueue(_queueCapacity);
            _threadPoolExecutor = new ThreadPoolExecutor(_corePoolSize, _maxPoolSize, _keepAliveTime, queue, _threadFactory, _rejectedExecutionHandler);
            if(_allowCoreThreadTimeOut) {
                _threadPoolExecutor.AllowsCoreThreadsToTimeOut = true;
            }
        }

        /// <summary>
        /// Create the BlockingQueue to use for the ThreadPoolExecutor.
        /// <p>A LinkedBlockingQueue instance will be created for a positive
        /// capacity value; a SynchronousQueue else.</p>
        /// </summary>
        /// <param name="queueCapacity">the specified queue capacity</param>
        /// <returns>the IBlockingQueue instance</returns>
        protected static IBlockingQueue<IRunnable> CreateQueue(int queueCapacity) {
            if(queueCapacity > 0) {
                return new LinkedBlockingQueue<IRunnable>(queueCapacity);
            }
            return new SynchronousQueue<IRunnable>();
        }

        /// <summary>
        /// get the underlying ThreadPoolExecutor for native access. (never <code>null</code>)
        /// </summary>
        /// <exception cref="InvalidOperationException">if the ThreadPoolTaskExecutor hasn't been initialized yet</exception>
        public ThreadPoolExecutor ThreadPoolExecutor {
            get {
                AssertUtils.State(_threadPoolExecutor != null, "ThreadPoolTaskExecutor not initialized");
                return _threadPoolExecutor;
            }
        }


        /// <summary>
        /// Implementation of the Spring IExecutor interface, delegating to the ThreadPoolExecutor instance.
        /// </summary>
        /// <param name="task"></param>
        public void Execute(IRunnable task) {
            Execute(task.Run);
        }     

        public void Execute(Action task) {
            IExecutor executor = ThreadPoolExecutor;
            try {
                executor.Execute(task);
            }
            catch(RejectedExecutionException ex) {
                throw new RejectedExecutionException("Executor [" + executor + "] did not accept task: " + task, ex);
            }
        }

        /// <summary>
        /// This task executor prefers short-lived work units.
        /// </summary>
        public bool PrefersShortLivedTasks {
            get { return true; }
        }

        /// <summary>
        /// Return the current pool size.
        /// </summary>
        public int PoolSize {
            get { return ThreadPoolExecutor.PoolSize; }
        }

        /// <summary>
        /// Return the number of currently active threads.
        /// </summary>
        public int ActiveCount {
            get { return ThreadPoolExecutor.ActiveCount; }
        }


        /**
         * Calls <code>shutdown</code> when the BeanFactory destroys
         * the task executor instance.
         * @see #shutdown()
         */
        public void Dispose() {
            Shutdown();
        }

        /// <summary>
        /// Perform a shutdown on the ThreadPoolExecutor.
        /// </summary>
        public void Shutdown() {
            if(logger.IsInfoEnabled) {
                logger.Info("Shutting down ThreadPoolExecutor" + (_objectName != null ? " '" + _objectName + "'" : ""));
            }
            if(_waitForTasksToCompleteOnShutdown) {
                _threadPoolExecutor.Shutdown();
            }
            else {
                _threadPoolExecutor.ShutdownNow();
            }
        }

        #region IExecutor Members


        #endregion
    }
}
