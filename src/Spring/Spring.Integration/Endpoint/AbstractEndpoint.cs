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
using Spring.Integration.Context;
using Spring.Objects.Factory;
using Spring.Threading.Locks;

namespace Spring.Integration.Endpoint {
    /// <summary>
    /// The base class for Message Endpoint implementations.
    /// 
    /// <p>This class implements Lifecycle and provides an {@link #autoStartup}
    /// property. If <code>true</code>, the endpoint will start automatically upon
    /// initialization. Otherwise, it will require an explicit invocation of its
    /// {@link #start()} method. The default value is <code>true</code>.
    /// To require explicit startup, provide a value of <code>false</code>
    /// to the {@link #setAutoStartup(boolean)} method.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractEndpoint : IntegrationObjectSupport, ILifecycle, IInitializingObject {

        private volatile bool autoStartup = true;

        private volatile bool running;

        private readonly ReentrantLock lifecycleLock = new ReentrantLock();


        public bool AutoStartup {
            set { autoStartup = value; }
        }

        #region IInitializingObject members

        public void AfterPropertiesSet() {
            try {
                OnInit();
                if(autoStartup) {
                    Start();
                }
            }
            catch(Exception ex) {
                throw new ObjectInitializationException("failed to initialize", ex);
            }
        }

        #endregion

        #region ILifecycle member

        public bool IsRunning {
            get {
                lifecycleLock.Lock();
                try {
                    return running;
                }
                finally {
                    lifecycleLock.Unlock();
                }
            }
        }

        public void Start() {
            lifecycleLock.Lock();
            try {
                if(!running) {
                    DoStart();
                    running = true;
                    if(logger.IsInfoEnabled) {
                        logger.Info("started " + this);
                    }
                }
            }
            finally {
                lifecycleLock.Unlock();
            }
        }

        public void Stop() {
            lifecycleLock.Lock();
            try {
                if(running) {
                    DoStop();
                    running = false;
                    if(logger.IsInfoEnabled) {
                        logger.Info("stopped " + this);
                    }
                }
            }
            finally {
                lifecycleLock.Unlock();
            }
        }
        #endregion

        protected virtual void OnInit() {
        }

        /**
         * Subclasses must implement this method with the start behavior.
         * This method will be invoked while holding the {@link #lifecycleLock}.
         */
        protected abstract void DoStart();

        /**
         * Subclasses must implement this method with the stop behavior.
         * This method will be invoked while holding the {@link #lifecycleLock}.
         */
        protected abstract void DoStop();
    }
}