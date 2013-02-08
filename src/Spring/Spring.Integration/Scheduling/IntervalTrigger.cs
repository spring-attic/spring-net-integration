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
    /// A trigger for periodic execution. The interval may be applied as either
    /// fixed-rate or fixed-delay, and an initial delay value may also be
    /// configured. The default initial delay is 0, and the default behavior is
    /// fixed-delay: each subsequent delay is measured from the last completion
    /// time. To enable execution between the scheduled start time of each
    /// execution, set 'fixedRate' to true.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class IntervalTrigger : ITrigger {

        private readonly TimeSpan interval;

        private TimeSpan initialDelay;

        private volatile bool fixedRate;

        /// <summary>
        /// Create a trigger with the given interval.
        /// </summary>
        /// <param name="interval">the internal</param>
        public IntervalTrigger(TimeSpan interval) {
            if(interval.TotalMilliseconds < 0)
                throw new ArgumentException("interval must not be negative");
            this.interval = interval;
        }

        /// <summary>
        /// Specify the delay for the initial execution.
        /// </summary>
        public TimeSpan InitialDelay {
            set {
                lock(this) {
                    initialDelay = value;
                }
            }
        }

        /// <summary>
        /// Specify whether the interval should be measured between the
        /// scheduled start times rather than between actual completion times
        /// (the latter, "fixed delay" behavior, is the default).
        /// </summary>
        public bool FixedRate {
            set { fixedRate = value; }
        }

        /// <summary>
        /// Returns the next time a task should run.
        /// </summary>
        /// <param name="lastScheduledRunTime">the <see cref="DateTime"/> the task was last scheduled</param>
        /// <param name="lastCompleteTime">the <see cref="DateTime"/> the task has latest completed</param>
        /// <returns>the next run <see cref="DateTime"/></returns>
        public DateTime GetNextRunTime(DateTime lastScheduledRunTime, DateTime lastCompleteTime) {
            // TODO investigate how to  call for DateTime.Now default(DateTIme) is no good solution
            if(lastScheduledRunTime == default(DateTime)) {
                return DateTime.Now + initialDelay;
            }

            if(fixedRate) {
                return lastScheduledRunTime + interval;
            }
            return lastCompleteTime + interval;
        }
    }
}
