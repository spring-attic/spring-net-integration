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

using Spring.Context;
using Spring.Threading;
using Spring.Threading.Future;

namespace Spring.Integration.Scheduling {
    /// <summary>
    /// Base interface for scheduling tasks.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Iwein Fuld</author>
    /// <author>Andreas D�hring (.NET)</author>
    public interface ITaskScheduler : ILifecycle {
        /// <summary>
        /// Schedules a task for multiple executions according to a Trigger.
        /// </summary>
        /// <param name="task">Task to be run multiple times</param>
        /// <param name="trigger">Trigger that determines at which times the task should be run</param>
        /// <returns></returns>
        IScheduledFuture<object> Schedule(IRunnable task, ITrigger trigger);
    }
}