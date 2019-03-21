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

using Common.Logging;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Threading;
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Dispatcher {
    /// <summary>
    /// Base class for <see cref="IMessageDispatcher"/> implementations.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractDispatcher : IMessageDispatcher {

        protected ILog logger = LogManager.GetLogger(typeof(AbstractDispatcher));

        // TODO should be CopyOnWriteArraySet
        protected CopyOnWriteArrayList<IMessageHandler> _handlers = new CopyOnWriteArrayList<IMessageHandler>();

        private volatile IExecutor _taskExecutor;

        #region IMessageDispatcher Members

        /// <summary>
        /// add a new <see cref="IMessageHandler"/> to the handler set
        /// </summary>
        /// <param name="handler">the handler to add</param>
        /// <returns><c>true</c> if the handler is added otherwise <c>false</c></returns>
        public bool AddHandler(IMessageHandler handler) {
            int count = _handlers.Count;
            _handlers.Add(handler);
            return _handlers.Count != count;
        }

        /// <summary>
        /// remove the <paramref name="handler from the set"/>
        /// </summary>
        /// <param name="handler">the handler to remove</param>
        /// <returns><c>true</c> if the <paramref name="handler"/> is removed otherwise <c>false</c></returns>
        public bool RemoveHandler(IMessageHandler handler) {
            return _handlers.Remove(handler);
        }

        /// <summary>
        /// dispatch the <paramref name="message"/>
        /// </summary>
        /// <param name="message">the message to dispatch</param>
        /// <returns><c>true</c> if the message is dispatched otherwise <c>false</c></returns>
        public abstract bool Dispatch(IMessage message);

        #endregion

        /// <summary>
        /// get/set a <see cref="IExecutor"/> for invoking the handlers.
        /// If none is provided, the invocation will occur in the thread
        /// that runs this polling dispatcher.
        /// </summary>
        public IExecutor TaskExecutor {
            protected get { return _taskExecutor; }
            set { _taskExecutor = value; }
        }

        public override string ToString() {
            return GetType().Name + " with handlers: " + _handlers;
        }

        /// <summary>
        /// Convenience method available for subclasses. Returns 'true' unless a
        /// "Selective Consumer" throws a <see cref="MessageRejectedException"/>.
        /// </summary>
        /// <param name="message">the message to handle</param>
        /// <param name="handler">the messagehandler</param>
        /// <returns></returns>
        protected bool SendMessageToHandler(IMessage message, IMessageHandler handler) {
            try {
                handler.HandleMessage(message);
                return true;
            }
            catch(MessageRejectedException ex) {
                #region logging
                if(logger.IsDebugEnabled) {
                    logger.Debug("Handler '" + handler + "' rejected Message, continuing with other handlers if available.", ex);
                }
                #endregion
            }
            return false;
        }
    }
}
