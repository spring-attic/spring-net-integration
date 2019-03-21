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
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// MessageHandler implementation that logs the Message payload. If the payload
    /// is assignable to Throwable, it will log the stack trace.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class LoggingHandler : AbstractMessageHandler {

        private enum Level { FATAL, ERROR, WARN, INFO, DEBUG, TRACE };


        private readonly Level _level;

        /// <summary>
        /// Create a LoggingHandler with the given log level (case-insensitive).
        /// <p>The valid levels are: FATAL, ERROR, WARN, INFO, DEBUG, or TRACE
        /// </summary>
        /// <param name="level"></param>
        public LoggingHandler(string level) {
            try {
                _level = (Level)Enum.Parse(typeof(Level), level, true);
            }
            catch(ArgumentException) {
                throw new ArgumentException("Invalid log level '" + level +
                                            "'. The (case-insensitive) supported values are: " +
                                            StringUtils.CollectionToCommaDelimitedString(Enum.GetValues(typeof(Level))));
            }
        }


        protected override void HandleMessageInternal(IMessage message) {
            object logMessage = message.Payload;
            if(logMessage is Exception) {
                logMessage = ((Exception)logMessage).Message + "\n" + ((Exception)logMessage).StackTrace;
            }
            switch(_level) {
                case Level.FATAL:
                    if(logger.IsFatalEnabled) {
                        logger.Fatal(logMessage);
                    }
                    break;
                case Level.ERROR:
                    if(logger.IsErrorEnabled) {
                        logger.Error(logMessage);
                    }
                    break;
                case Level.WARN:
                    if(logger.IsWarnEnabled) {
                        logger.Warn(logMessage);
                    }
                    break;
                case Level.INFO:
                    if(logger.IsInfoEnabled) {
                        logger.Info(logMessage);
                    }
                    break;
                case Level.DEBUG:
                    if(logger.IsDebugEnabled) {
                        logger.Debug(logMessage);
                    }
                    break;
                case Level.TRACE:
                    if(logger.IsTraceEnabled) {
                        logger.Trace(logMessage);
                    }
                    break;
            }
        }
    }
}
