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
//using System.Collections.Generic;
using System.Collections.Generic;
using AopAlliance.Aop;
using Spring.Threading;
using Spring.Transaction;

namespace Spring.Integration.Scheduling {
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PollerMetadata {

        private volatile ITrigger trigger;

        private volatile int maxMessagesPerPoll;

        private /*volatile*/ TimeSpan receiveTimeout = new TimeSpan(0, 0, 0, 0, 1000);

        private IList<IAdvice> adviceChain;

        private volatile IExecutor taskExecutor;

        private volatile IPlatformTransactionManager transactionManager;

        private volatile ITransactionDefinition transactionDefinition;


        public ITrigger Trigger {
            get { return trigger; }
            set { trigger = value; }
        }

        public int MaxMessagesPerPoll {
            get { return maxMessagesPerPoll; }
            set { maxMessagesPerPoll = value; }
        }

        public TimeSpan ReceiveTimeout {
            get {
                lock(this) {
                    return receiveTimeout;
                }
            }
            set {
                lock(this) {
                    receiveTimeout = value;
                }
            }
        }

        public IList<IAdvice> AdviceChain {
            get { return adviceChain; }
            set {
                adviceChain = value;
            }
        }

        public IExecutor TaskExecutor {
            get { return taskExecutor; }
            set { taskExecutor = value; }
        }

        public IPlatformTransactionManager TransactionManager {
            get { return transactionManager; }
            set { transactionManager = value; }
        }

        public ITransactionDefinition TransactionDefinition {
            get { return transactionDefinition; }
            set { transactionDefinition = value; }
        }
    }
}
