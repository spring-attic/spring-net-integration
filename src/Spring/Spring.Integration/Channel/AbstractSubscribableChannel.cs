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
using Spring.Integration.Core;
using Spring.Integration.Dispatcher;
using Spring.Integration.Message;
using Spring.Util;

namespace Spring.Integration.Channel {
    /// <summary>
    /// Base implementation of <see cref="IMessageChannel"/> that invokes the subscribed
    /// <see cref="IMessageHandler"/> by delegating to a {@link MessageDispatcher}.
    /// </summary>
    /// <typeparam name="T">the type of the dispatcher</typeparam>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class AbstractSubscribableChannel<T> : AbstractMessageChannel, ISubscribableChannel where T : IMessageDispatcher {

        private readonly T _dispatcher;

        /// <summary>
        /// create a new <see cref="AbstractSubscribableChannel{T}" with the <paramref name="dispatcher"/>/>
        /// </summary>
        /// <param name="dispatcher">the dispatcher for the <see cref="AbstractSubscribableChannel{T}"/></param>
        public AbstractSubscribableChannel(T dispatcher) {
            AssertUtils.ArgumentNotNull(dispatcher, "dispatcher", "must not be null");
            _dispatcher = dispatcher;
        }


        /// <summary>
        /// get the <see cref="IMessageDispatcher"/>
        /// </summary>
        protected T Dispatcher {
            get { return _dispatcher; }
        }

        /// <summary>
        /// subscribe <paramref name="handler"/> to the <see cref="Dispatcher"/>
        /// </summary>
        /// <param name="handler">the handler to subscribe</param>
        /// <returns><c>true</c> if the handler is subscribed otherwise <c>false</c></returns>
        public bool Subscribe(IMessageHandler handler) {
            return _dispatcher.AddHandler(handler);
        }

        /// <summary>
        /// unsubscribe the <paramref name="handler"/>
        /// </summary>
        /// <param name="handler">the handler to unsubscribe</param>
        /// <returns><c>true</c> if the handler is unsubscribed otherwise <c>false</c></returns>
        public bool Unsubscribe(IMessageHandler handler) {
            return _dispatcher.RemoveHandler(handler);
        }

        /// <summary>
        /// dispatch the <paramref name="message"/>
        /// </summary>
        /// <param name="message">the message to dispatch</param>
        /// <param name="timeout">ignored</param>
        /// <returns></returns>
        protected override bool DoSend(IMessage message, TimeSpan timeout) {
            return _dispatcher.Dispatch(message);
        }
    }
}
