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
using Common.Logging;
using Spring.Core;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Util;

namespace Spring.Integration.Handler {
    /// <summary>
    /// Base class for MessageHandler implementations that provides basic
    /// validation and error handling capabilities. Asserts that the incoming
    /// Message is not null and that it does not contain a null payload. Converts
    /// checked exceptions into runtime {@link MessagingException}s. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractMessageHandler : IMessageHandler, IOrdered {

        protected ILog logger = LogManager.GetLogger(typeof(AbstractMessageHandler));

        private volatile int order = int.MaxValue; //lowest precidence

        public void HandleMessage(IMessage message) {
            AssertUtils.ArgumentNotNull(message, "Message must not be null");
            AssertUtils.ArgumentNotNull(message.Payload, "Message payload must not be null");
            if(logger.IsDebugEnabled) {
                logger.Debug(this + " received message: " + message);
            }
            try {
                HandleMessageInternal(message);
            }
            catch(Exception ex) {
                if(ex is MessagingException) {
                    throw;
                }
                throw new MessageHandlingException(message, "error occurred in message handler [" + this + "]", ex);
            }
        }

        protected abstract void HandleMessageInternal(IMessage message);

        #region Implementation of IOrdered

        public int Order
        {
            get { return order; }
            set { order = value; }
        }

        #endregion
    }
}
