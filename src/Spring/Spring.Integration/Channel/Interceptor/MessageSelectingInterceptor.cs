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

using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Selector;

namespace Spring.Integration.Channel.Interceptor {
    /// <summary>
    /// A <see cref="IChannelInterceptor"/>  that delegates to a list of <see cref="IMessageSelector"/>s
    /// to decide whether a <see cref="IMessage"/> should be accepted on the <see cref="IMessageChannel"/>.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class MessageSelectingInterceptor : ChannelInterceptorAdapter {

        private readonly IList<IMessageSelector> _selectors;

        /// <summary>
        /// create a new <see cref="MessageSelectingInterceptor"/>
        /// </summary>
        /// <param name="selectors">a list of <see cref="IMessageSelector"/></param>
        public MessageSelectingInterceptor(params IMessageSelector[] selectors) {
            _selectors = selectors;
        }

        /// <summary>
        /// ask each <see cref="IMessageSelector"/> whether it acceppts the <see cref="IMessage"/>
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="channel">ignored</param>
        /// <returns>the message, if all <see cref="IMessageSelector"/>s accept the message</returns>
        /// <exception cref="MessageDeliveryException">if one <see cref="IMessageSelector"/> does not accept the <paramref name="message"/></exception>
        public override IMessage PreSend(IMessage message, IMessageChannel channel) {
            foreach(IMessageSelector selector in _selectors) {
                if(!selector.Accept(message)) {
                    throw new MessageDeliveryException(message, "selector '" + selector + "' did not accept message");
                }
            }
            return message;
        }
    }
}
