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
using Spring.Threading;
using Spring.Util;

namespace Spring.Integration.Util {

    /// <summary>
    /// A {@link TaskExecutor} implementation that wraps an existing TaskExecutor
    /// instance in order to catch any exceptions. If an exception is thrown, it
    /// will be handled by the provided {@link ErrorHandler}.
    /// </summary>
    /// <author>Jonas Partner</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ErrorHandlingTaskExecutor : IExecutor {

        private readonly IExecutor _taskExecutor;
        private readonly IErrorHandler _errorHandler;

        /// <summary>
        /// create a new <see cref="ErrorHandlingTaskExecutor"/> with <paramref name="taskExecutor"/> and
        /// <paramref name="errorHandler"/>
        /// </summary>
        /// <param name="taskExecutor">the task executor</param>
        /// <param name="errorHandler">the error handler in case of an exception</param>
        public ErrorHandlingTaskExecutor(IExecutor taskExecutor, IErrorHandler errorHandler) {
            AssertUtils.ArgumentNotNull(taskExecutor, "taskExecutor must not be null");
            AssertUtils.ArgumentNotNull(errorHandler, "errorHandler must not be null");
            _taskExecutor = taskExecutor;
            _errorHandler = errorHandler;
        }

        #region IExecutor Members

        public void Execute(IRunnable task) {
            Execute(task.Run);
        }
        
        public void Execute(Action task) {
            _taskExecutor.Execute(() => {
                try {
                    task();
                }
                catch(Exception ex) {
                    _errorHandler.HandleError(ex);
                }
            });
        }

        #endregion
    }
}