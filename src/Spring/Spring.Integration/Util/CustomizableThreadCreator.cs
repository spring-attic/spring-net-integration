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
using System.Threading;
using Spring.Threading;

namespace Spring.Integration.Util {

    /// <summary>
    /// Simple customizable helper class for creating threads. Provides various
    /// bean properties, such as thread name prefix, thread priority, etc.
    /// 
    /// <p>Serves as base class for thread factories such as
    /// {@link org.springframework.scheduling.concurrent.CustomizableThreadFactory}.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class CustomizableThreadCreator {

        private string _threadNamePrefix;

        private ThreadPriority _threadPriority = ThreadPriority.Normal;

        private bool _daemon;

        //private ThreadGroup threadGroup;

        private int _threadCount;

        private readonly object _threadCountMonitor = new object();

        /// <summary>
        /// Create a new CustomizableThreadCreator with default thread name prefix.
        /// </summary>
        public CustomizableThreadCreator() {
            _threadNamePrefix = DefaultThreadNamePrefix;
        }

        /// <summary>
        /// Create a new CustomizableThreadCreator with the given thread name prefix.
        /// </summary>
        /// <param name="threadNamePrefix">the prefix to use for the names of newly created threads</param>
        public CustomizableThreadCreator(String threadNamePrefix) {
            _threadNamePrefix = (threadNamePrefix != null ? threadNamePrefix : DefaultThreadNamePrefix);
        }


        /// <summary>
        /// get/set the name prefrix for newly created threads
        /// </summary>
        public string ThreadNamePrefix {
            get { return _threadNamePrefix; }
            set { _threadNamePrefix = value != null ? value : DefaultThreadNamePrefix; }
        }

        /// <summary>
        /// get/set the priority of the threads that this factory creates.
        /// </summary>
        public ThreadPriority ThreadPriority {
            get { return _threadPriority; }
            set { _threadPriority = value; }
        }


        /// <summary>
        /// get/set whether this factory is supposed to create daemon threads,
        /// just executing as long as the application itself is running.
        /// <p>Default is "false": Concrete factories usually support explicit
        /// cancelling. Hence, if the application shuts down, Runnables will
        /// by default finish their execution.
        /// <p>Specify "true" for eager shutdown of threads which still
        /// actively execute a Runnable.
        /// </summary>
        public bool Daemon {
            get { return _daemon; }
            set { _daemon = value; }
        }


        ///**
        // * Specify the name of the thread group that threads should be created in.
        // * @see #setThreadGroup
        // */
        //public void setThreadGroupName(String name) {
        //    this.threadGroup = new ThreadGroup(name);
        //}

        ///**
        // * Specify the thread group that threads should be created in.
        // * @see #setThreadGroupName
        // */
        //public void setThreadGroup(ThreadGroup threadGroup) {
        //    this.threadGroup = threadGroup;
        //}

        ///**
        // * Return the thread group that threads should be created in
        // * (or <code>null</code>) for the default group.
        // */
        //public ThreadGroup getThreadGroup() {
        //    return this.threadGroup;
        //}

        /// <summary>
        /// Template method for the creation of a Thread.
        /// <p>Default implementation creates a new Thread for the given
        /// Runnable, applying an appropriate thread name.
        /// </summary>
        /// <param name="runnable">runnable the Runnable to execute</param>
        /// <returns>a newly created but not started thread</returns>
        public Thread CreateThread(IRunnable runnable) {
            Thread thread = new Thread(runnable.Run);
            thread.Name = NextThreadName;
            thread.Priority = ThreadPriority;
            thread.IsBackground = Daemon;
            return thread;
        }

        /// <summary>
        /// Return the thread name to use for a newly created thread.
        /// <p>Default implementation returns the specified thread name prefix
        /// with an increasing thread count appended: for example,
        /// "SimpleAsyncTaskExecutor-0".
        /// </summary>
        protected string NextThreadName {
            get {
                int threadNumber;
                lock(_threadCountMonitor) {
                    _threadCount++;
                    threadNumber = _threadCount;
                }
                return ThreadNamePrefix + threadNumber;
            }
        }

        /// <summary>
        /// Build the default thread name prefix for this factory. (never <code>null</code>)
        /// </summary>
        protected string DefaultThreadNamePrefix {
            get { return GetType().Name + "-"; }
        }
    }
}