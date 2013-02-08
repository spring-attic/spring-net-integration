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

namespace Spring.Integration.Scheduling {
    /// <summary>
    /// A strategy for providing the next time a task should run.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public interface ITrigger {
        /// <summary>
        /// Returns the next time that a task should run or <c>null</c> if the task should not run again.
        /// </summary>
        /// <param name="lastScheduledRunTime">
        /// last time the relevant task was scheduled to run, or <c>null</c> if it has never been scheduled
        /// </param>
        /// <param name="lastCompleteTime">
        /// last time the relevant task finished or <c>null</c> if it did not run to completion
        /// </param>
        /// <returns>next time that a task should run</returns>
        DateTime GetNextRunTime(DateTime lastScheduledRunTime, DateTime lastCompleteTime);
    }
}