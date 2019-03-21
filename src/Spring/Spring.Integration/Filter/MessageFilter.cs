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

using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Selector;
using Spring.Util;

namespace Spring.Integration.Filter {
    /// <summary>
    /// Message Handler that delegates to a {@link MessageSelector}. If and only if
    /// the selector {@link MessageSelector#accept(Message) accepts} the Message, it
    /// will be passed to this filter's output channel. Otherwise the message will
    /// either be silently dropped (the default) or will trigger the throwing of a
    /// {@link MessageRejectedException} depending on the value of its
    /// {@link #throwExceptionOnRejection} property.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageFilter : AbstractReplyProducingMessageHandler {

        private readonly IMessageSelector _selector;

        private volatile bool _throwExceptionOnRejection;

        /// <summary>
        /// Create a MessageFilter that will delegate to the given
        /// </summary>
        /// <param name="selector"></param>
        public MessageFilter(IMessageSelector selector) {
            AssertUtils.ArgumentNotNull(selector, "selector must not be null");
            _selector = selector;
        }

        /// <summary>
        /// Specify whether this filter should throw a 
        /// {@link MessageRejectedException} when its selector does not accept a
        /// Message. The default value is <code>false</code> meaning that rejected
        /// Messages will be quietly dropped.
        /// </summary>
        public bool ThrowExceptionOnRejection {
            set { _throwExceptionOnRejection = value; }
        }

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
            if(_selector.Accept(message)) {
                replyHolder.Set(message);
            }
            else if(_throwExceptionOnRejection) {
                throw new MessageRejectedException(message);
            }
        }
    }
}
