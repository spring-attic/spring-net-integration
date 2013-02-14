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
using Quartz;

namespace Spring.Integration.Scheduling {
    /// <summary>
    /// A trigger that uses a cron expression. See {@link CronSequenceGenerator}
    /// for a detailed description of the expression pattern syntax.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    /// <author>Anindya Chatterjee (.NET)</author>
    public class CronTrigger : ITrigger {

        private readonly string _expression;
        private readonly CronExpression _cronExpression;


        /// <summary>
        /// Create a trigger for the given cron expression.
        /// <see cref="Quartz.CronExpression"/>
        /// </summary>
        public CronTrigger(string expression) {
            _expression = expression;
            _cronExpression = new CronExpression(expression);
        }

        /// <summary>
        /// Return the next time a task should run. Determined by consulting this
        /// trigger's cron expression compared with the lastCompleteTime. If the
        /// lastCompleteTime is <code>default(DateTime)</code>, the current time is used.
        /// </summary>
        /// <param name="lastScheduledRunTime">ignored</param>
        /// <param name="lastCompleteTime">the last complete time of the trigger or default(DateTime)</param>
        /// <returns>the next scheduled time</returns>
        public DateTime GetNextRunTime(DateTime lastScheduledRunTime, DateTime lastCompleteTime) {
            DateTime dateTime = (lastCompleteTime != default(DateTime)) ? lastCompleteTime : DateTime.Now;

            DateTime? newDateTime;

            DateTimeOffset? offset = _cronExpression.GetNextValidTimeAfter(dateTime);
            if(offset.HasValue)
            {
                DateTimeOffset value = offset.Value;
                DateTime dateTimeValue = value.DateTime;
                newDateTime = dateTimeValue;
            }
            else
            {
                newDateTime = default(DateTime?);
            }
            

            if(!newDateTime.HasValue)
                throw new ArgumentException(dateTime + " has no next value matching the pattern [" + _expression + "]");

            return newDateTime.Value;
        }

        //@Override
        //public boolean equals(Object other) {
        //    if (other == null || !(other instanceof CronTrigger)) {
        //        return false;
        //    }
        //    return this.cronSequenceGenerator.equals(
        //            ((CronTrigger) other).cronSequenceGenerator);
        //}

        //@Override
        //public int hashCode() {
        //    return this.cronSequenceGenerator.hashCode();
        //}
    }
}
