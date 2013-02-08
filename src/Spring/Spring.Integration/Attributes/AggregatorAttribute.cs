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
using Spring.Integration.Aggregator;
using Spring.Integration.Util;

namespace Spring.Integration.Attributes {
    /// <summary>
    /// Indicates that a method is capable of aggregating messages. 
    /// <p>
    /// A method annotated with @Aggregator may accept a collection
    /// of Messages or Message payloads and should return a single
    /// Message or a single Object to be used as a Message payload.
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    [AttributeUsage(AttributeTargets.Method)]
    public class AggregatorAttribute : Attribute {
        private string _inputChannel = "";
        private string _outputChannel = "";
        private string _discardChannel = "";
        private TimeSpan _sendTimeOut = AbstractMessageAggregator.DEFAULT_SEND_TIMEOUT;
        private TimeSpan _timeOut = AbstractMessageAggregator.DEFAULT_TIMEOUT;
        private bool _sendPartialResultsOnTimeout;
        private TimeSpan _reaperInterval = AbstractMessageAggregator.DEFAULT_REAPER_INTERVAL;
        private int _trackedCorrelationIdCapacity = AbstractMessageAggregator.DEFAULT_TRACKED_CORRRELATION_ID_CAPACITY;

        /// <summary>
        /// channel name for receiving messages to be aggregated
        /// </summary>
        public string InputChannel {
            get { return _inputChannel; }
            set { _inputChannel = value; }
        }

        /// <summary>
        /// channel name for sending aggregated result messages
        /// </summary>
        public string OutputChannel {
            get { return _outputChannel; }
            set { _outputChannel = value; }
        }

        /// <summary>
        /// channel name for sending discarded messages (due to a timeout)
        /// </summary>
        public string DiscardChannel {
            get { return _discardChannel; }
            set { _discardChannel = value; }
        }

        /// <summary>
        /// timeout for sending results to the reply target (in milliseconds)
        /// </summary>
        public TimeSpan SendTimeOut {
            get { return _sendTimeOut; }
            set { _sendTimeOut = value; }
        }

        /// <summary>
        /// maximum time to wait for completion (in milliseconds) 
        /// </summary>
        public TimeSpan TimeOut {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        /// <summary>
        /// indicates whether to send an incomplete aggregate on timeout
        /// </summary>
        public bool SendPartialResultsOnTimeout {
            get { return _sendPartialResultsOnTimeout; }
            set { _sendPartialResultsOnTimeout = value; }
        }

        /// <summary>
        /// interval for the task that checks for timed-out aggregates
        /// </summary>
        public TimeSpan ReaperInterval {
            get { return _reaperInterval; }
            set { _reaperInterval = value; }
        }

        /// <summary>
        /// maximum number of correlation IDs to maintain so that received messages
        /// may be recognized as belonging to an aggregate that has already completed
        /// or timed out
        /// </summary>
        public int TrackedCorrelationIdCapacity {
            get { return _trackedCorrelationIdCapacity; }
            set { _trackedCorrelationIdCapacity = value; }
        }
    }
}
