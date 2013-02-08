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
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Spring.Context;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Future;
using Spring.Threading.Locks;
using Spring.Util;

namespace Spring.Integration.Scheduling {
    /// <summary>
    /// An implementation of <see cref="ITaskScheduler"/> that delegates to any instance
    /// of <see cref="IExecutor"/> 
    /// 
    /// <p>This class implements Lifecycle and provides an {@link #autoStartup}
    /// property. If <code>true</code>, the scheduler will start automatically upon
    /// receiving the {@link ContextRefreshedEvent}. Otherwise, it will require an
    /// explicit invocation of its {@link #start()} method. The default value is
    /// <code>true</code>. To require explicit startup, provide a value of
    /// <code>false</code> to the {@link #setAutoStartup(boolean)} method.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class SimpleTaskScheduler : ITaskScheduler, IObjectFactoryAware, IApplicationEventListener, IDisposable { //}DisposableBean {

        private readonly ILog logger = LogManager.GetLogger(typeof(SimpleTaskScheduler));

        private bool _isDisposed;
        private IExecutor _executor;

        private volatile bool _autoStartup = true;

        private volatile IErrorHandler _errorHandler;

        private volatile SchedulerTask _schedulerTask;

        private readonly DelayQueue<TriggeredTask> _scheduledTasks = new DelayQueue<TriggeredTask>();

        //private Set<TriggeredTask<?>> executingTasks = Collections.synchronizedSet(new TreeSet<TriggeredTask<?>>());
        
        //MLP private readonly IList<TriggeredTask<object>> _executingTasks = new List<TriggeredTask<object>>();
        //private readonly System.Collections.IList _executingTasks = new System.Collections.ArrayList();

        private readonly Spring.Collections.ISet _executingTasks = new Spring.Collections.SynchronizedSet(new Spring.Collections.SortedSet());

        private volatile bool _running;

        private readonly ReentrantLock _lifecycleLock = new ReentrantLock();


        /// <summary>
        /// creates a new <see cref="SimpleTaskScheduler"/>
        /// </summary>
        /// <param name="executor">the excutor to excute the tasks</param>
        public SimpleTaskScheduler(IExecutor executor) {
            AssertUtils.ArgumentNotNull(executor, "executor", "executor must not be null");
            _executor = executor;
        }

        #region ITaskScheduler Members

        public IScheduledFuture<object> Schedule(IRunnable task, ITrigger trigger) {
            AssertUtils.ArgumentNotNull(task, "task must not be null");
            TriggeredTask triggeredTask = new TriggeredTask(this, task, trigger);
            return Schedule(triggeredTask, DateTime.MinValue, DateTime.MinValue);
        }

        #endregion

        #region ILifecycle Members

        public bool IsRunning {
            get {
                _lifecycleLock.Lock();
                try {
                    return _running;
                }
                finally {
                    _lifecycleLock.Unlock();
                }
            }
        }

        public void Start() {
            _lifecycleLock.Lock();
            try {
                if(!_running) {
                    _executor.Execute(_schedulerTask = new SchedulerTask(this));
                    _running = true;
                    if(logger.IsInfoEnabled) {
                        logger.Info("started " + this);
                    }
                }
            }
            finally {
                _lifecycleLock.Unlock();
            }

        }

        public void Stop() {
            _lifecycleLock.Lock();
            try {
                if(_running) {
                    _schedulerTask.Deactivate();
                    Thread executingThread = _schedulerTask.ExecutingThread.Value;
                    if(executingThread != null) {
                        executingThread.Interrupt();
                    }
                    _scheduledTasks.Clear();
                    lock(_executingTasks) {
                        foreach(TriggeredTask task in _executingTasks) {
                            task.Cancel(true);
                        }
                        _executingTasks.Clear();
                    }
                    _schedulerTask = null;
                    _running = false;
                    if(logger.IsInfoEnabled) {
                        logger.Info("stopped " + this);
                    }
                }
            }
            finally {
                _lifecycleLock.Unlock();
            }

        }

        #endregion

        #region IObjectFactoryAware Members

        public IObjectFactory ObjectFactory {
            set {
                //if(_errorHandler == null) {
                //    _errorHandler = new MessagePublishingErrorHandler<>(new BeanFactoryChannelResolver(beanFactory));
                //}
            }
        }

        #endregion

        #region IApplicationEventListener Members

        public void HandleApplicationEvent(object sender, ApplicationEventArgs e) {
            if(_autoStartup)
                Start();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            lock(this) {
                if(!_isDisposed) {
                    if(disposing) {
                        Stop();
                        IDisposable disposableExecutor = _executor as IDisposable;
                        if(disposableExecutor != null) {
                            if(logger.IsInfoEnabled) {
                                logger.Info("shutting down TaskExecutor");
                            }
                            disposableExecutor.Dispose();
                        }
                    }

                    _executor = null;
                    _isDisposed = true;
                }
            }
        }

        #endregion

        private IScheduledFuture<object> Schedule(TriggeredTask triggeredTask, DateTime lastScheduledRunTime, DateTime lastCompleteTime) {
            DateTime nextRunTime = triggeredTask.Trigger.GetNextRunTime(lastScheduledRunTime, lastCompleteTime);
            if(nextRunTime != DateTime.MinValue) {
                triggeredTask.ScheduledTime = nextRunTime;
                _scheduledTasks.Offer(triggeredTask);
            }
            return triggeredTask;
        }

        /// <summary>
        /// set whether the scheduler should start automatically
        /// </summary>
        public bool AutoStartup {
            set { _autoStartup = value; }
        }

        /// <summary>
        /// set the error handler
        /// </summary>
        public IErrorHandler ErrorHandler {
            set { _errorHandler = value; }
        }

        /// <summary>
        /// always true
        /// </summary>
        /// <returns><c>true</c></returns>
        public bool PrefersShortLivedTasks() {
            return true;
        }

        /// <summary>
        /// exceute the <paramref name="task"/>
        /// </summary>
        /// <param name="task">the task to execute</param>
        public void Execute(IRunnable task) {
            _executor.Execute(task);
        }

        #region private classes

        private class SchedulerTask : IRunnable {
            private readonly SimpleTaskScheduler _outer;
            private readonly AtomicReference<Thread> _executingThread = new AtomicReference<Thread>();

            private volatile bool _active = true;

            public SchedulerTask(SimpleTaskScheduler outer) {
                _outer = outer;
            }

            public AtomicReference<Thread> ExecutingThread {
                get { return _executingThread; }
            }

            public void Run() {
                if(!ExecutingThread.CompareAndSet(null, Thread.CurrentThread)) {
                    throw new SchedulingException("The SchedulerTask is already running.");
                }
                while(_active) {
                    try {
                        TriggeredTask task = _outer._scheduledTasks.Take();
                        //if this thread is not active anymore, clear
                        if(_active) {
                            _outer._executor.Execute(task);
                        }
                        else {
                            _outer._scheduledTasks.Offer(task);
                        }
                    }
                    catch(ThreadInterruptedException) {
                        Thread.CurrentThread.Interrupt();
                        break;
                    }
                }
                ExecutingThread.Value = null;
            }

            public void Deactivate() {
                _active = false;
            }
        }

        /// <summary>
        /// Wrapper class that enables rescheduling of a task based on a Trigger.
        /// </summary>
        private class TriggeredTask : FutureTask<object>, IScheduledFuture<object>  {
            private readonly SimpleTaskScheduler _outer;
            private readonly ITrigger _trigger;

            private DateTime _scheduledTime;


            public TriggeredTask(SimpleTaskScheduler outer, IRunnable task, ITrigger trigger)
                : base(new ErrorHandlingRunnableWrapper(outer, task), null) {

                _outer = outer;
                _trigger = trigger;
            }

            public ITrigger Trigger {
                get { return _trigger; }
            }

            public DateTime ScheduledTime {
                set {
                    lock(this) {
                        _scheduledTime = value;
                    }
                }
            }

            //TODO MLP why 'new' and not 'override'?
            public override void Run() {
                _outer._executingTasks.Add(this);
                base.RunAndReset();
                _outer._executingTasks.Remove(this);
                if(_outer.IsRunning && !IsCancelled) {
                    _outer.Schedule(this, _scheduledTime, DateTime.Now);
                }
            }

            public override bool Cancel(bool mayInterruptIfRunning) {
                lock(this) {
                    if(!IsCancelled) {
                        _outer._scheduledTasks.Remove(this);
                    }
                    return base.Cancel(mayInterruptIfRunning);
                }
            }

            #region IDelayed Members

            public TimeSpan GetRemainingDelay() {
                DateTime now = DateTime.Now;
                DateTime scheduled = (_scheduledTime != DateTime.MinValue) ? _scheduledTime : now;

                return (scheduled > now) ? scheduled.Subtract(now) : TimeSpan.Zero;
            }

            #endregion

            #region IComparable<IDelayed> Members

            public int CompareTo(IDelayed other) {
                return GetRemainingDelay().CompareTo(other.GetRemainingDelay());
            }

            #endregion

            #region IComparable Members

            public int CompareTo(object obj) {
                return CompareTo((IDelayed)obj);
            }

            #endregion
        }

        /**
         * Wrapper that catches any Throwable thrown by a target task and
         * delegates to the {@link ErrorHandler} if available. If no error handler
         * has been configured, the error will be logged at error-level.
         */
        private class ErrorHandlingRunnableWrapper : IRunnable {
            private readonly SimpleTaskScheduler _outer;
            private readonly IRunnable _target;

            public ErrorHandlingRunnableWrapper(SimpleTaskScheduler outer, IRunnable target) {
                _outer = outer;
                _target = target;
            }

            public void Run() {
                try {
                    _target.Run();
                }
                catch(Exception ex) {
                    if(_outer._errorHandler != null) {
                        _outer._errorHandler.HandleError(ex);
                    }
                    else if(_outer.logger.IsErrorEnabled) {
                        _outer.logger.Error("Error occurred in task but no 'errorHandler' is available.", ex);
                    }
                }
            }
        }

        #endregion
    }
}
