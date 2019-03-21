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
using System.Collections.Generic;
using Common.Logging;
using Spring.Integration.Core;
using Spring.Integration.Selector;

namespace Spring.Integration.Channel {
    /// <summary>
    /// A channel implementation that essentially behaves like "/dev/null".
    /// All receive() calls will return <em>null</em>, and all send() calls
    /// will return <em>true</em> although no action is performed.
    /// Note however that the invocations are logged at debug-level.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class NullChannel : IPollableChannel {

        private readonly ILog logger = LogManager.GetLogger(typeof(NullChannel));

        /// <summary>
        /// Always returns <code>null</code>.
        /// </summary>
        public string Name {
            get { return null; }
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <returns>always <c>null</c></returns>
        public IList<IMessage> Clear() {
            return null;
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <param name="selector">ignored</param>
        /// <returns>always <c>null</c></returns>
        public IList<IMessage> Purge(IMessageSelector selector) {
            return null;
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <returns>always <c>null</c></returns>
        public IMessage Receive() {
            #region logging
            if(logger.IsDebugEnabled) {
                logger.Debug("receive called on null-channel");
            }
            #endregion
            return null;
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <param name="timeout">ignored</param>
        /// <returns>always <c>null</c></returns>
        public IMessage Receive(TimeSpan   timeout) {
            return Receive();
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <param name="message">ignored</param>
        /// <returns>always <c>null</c></returns>
        public bool Send(IMessage message) {
            #region logging
            if(logger.IsDebugEnabled) {
                logger.Debug("message sent to null-channel: " + message);
            }
            #endregion
            return true;
        }

        /// <summary>
        /// noop
        /// </summary>
        /// <param name="message">ignored</param>
        /// <param name="timeout">ignored</param>
        /// <returns>always <c>null</c></returns>
        public bool Send(IMessage message, TimeSpan timeout) {
            return Send(message);
        }
    }
}
