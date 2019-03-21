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
using Spring.Integration.Scheduling;
using Spring.Integration.Util;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Threading.Collections;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;
using Spring.Util;

namespace Spring.Integration.Context {

    /// <summary>
    /// 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    /// <author>Anindya Chatterjee (.NET)</author>
    public abstract class IntegrationContextUtils {
        public const string TaskSchedulerObjectName = "taskScheduler";

        public const string ErrorChannelObjectName = "errorChannel";

        public const string NullChannelObjectName = "nullChannel";

        public const string DefaultPollerMetadataObjectName = "spring.integration.context.defaultPollerMetadata";

        public const string LoggingHandlerObjectName = "loggingHandler";

        /// <summary>
        /// get the error channel object
        /// </summary>
        /// <param name="objectFactory">the object factory to get the error channel from</param>
        /// <returns></returns>
        public static IMessageChannel GetErrorChannel(IObjectFactory objectFactory) {
            return GetObjectOfType<IMessageChannel>(objectFactory, ErrorChannelObjectName);
        }

        public static ITaskScheduler GetTaskScheduler(IObjectFactory objectFactory) {
            return GetObjectOfType<ITaskScheduler>(objectFactory, TaskSchedulerObjectName);
        }

        public static ITaskScheduler GetRequiredTaskScheduler(IObjectFactory objectFactory) {
            ITaskScheduler taskScheduler = GetTaskScheduler(objectFactory);
            AssertUtils.State(taskScheduler != null, "No such object '" + TaskSchedulerObjectName + "'");
            return taskScheduler;
        }

        public static PollerMetadata GetDefaultPollerMetadata(IObjectFactory objectFactory) {
            return GetObjectOfType<PollerMetadata>(objectFactory, DefaultPollerMetadataObjectName);
        }

        private static T GetObjectOfType<T>(IObjectFactory objectFactory, string objectName) where T : class {
            if(!objectFactory.ContainsObject(objectName)) {
                return null;
            }
            object obj = objectFactory.GetObject(objectName);
            AssertUtils.State(obj is T, "incorrect type for object '" + objectName
                        + "' expected [" + typeof(T) + "], but actual type is [" + obj.GetType() + "].");

            return (T)obj;
        }

        public static IExecutor CreateThreadPoolTaskExecutor(int coreSize, int maxSize, int queueCapacity, string threadPrefix)
        {
            
            IBlockingQueue<IRunnable> queue = queueCapacity > 0 ? new LinkedBlockingQueue<IRunnable>(queueCapacity) : new LinkedBlockingQueue<IRunnable>();
            var executor = new ThreadPoolExecutor(coreSize, maxSize, TimeSpan.Zero, queue);
            if(StringUtils.HasText(threadPrefix)) {
                // TODO new CustomizableThreadFactory(threadPrefix);
                //executor.ThreadFactory = new CustomizableThreadFactory(threadPrefix);
            }

            executor.RejectedExecutionHandler = new ThreadPoolExecutor.CallerRunsPolicy();
            // TODO executor.AfterPropertiesSet();
            return executor;
            

            /*

            ThreadPoolTaskExecutor executor = new ThreadPoolTaskExecutor();
            executor.CorePoolSize = coreSize;
            executor.MaxPoolSize = maxSize;
            executor.QueueCapacity = queueCapacity;
            if (StringUtils.HasText(threadPrefix))
            {
                executor.ThreadFactory = new CustomizableThreadFactory(threadPrefix);
            }
            executor.RejectedExecutionHandler = new CallerRunsPolicy();
            executor.AfterPropertiesSet();
            return executor;
             */
        }
    }
}
