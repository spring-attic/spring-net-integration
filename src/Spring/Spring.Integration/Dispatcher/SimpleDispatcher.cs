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

using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Dispatcher {
    /// <summary>
    /// Basic implementation of {@link MessageDispatcher} that will attempt
    /// to send a {@link Message} to one of its handlers. As soon as <em>one</em>
    /// of the handlers accepts the Message, the dispatcher will return 'true'.
    /// <p>
    /// If the dispatcher has no handlers, a {@link MessageDeliveryException}
    /// will be thrown. If all handlers reject the Message, the dispatcher will
    /// throw a MessageRejectedException.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class SimpleDispatcher : AbstractDispatcher {

        public override bool Dispatch(IMessage message) {
            if(_handlers.Count == 0) {
                throw new MessageDeliveryException(message, "Dispatcher has no subscribers.");
            }
            int count = 0;
            int rejectedExceptionCount = 0;
            foreach(IMessageHandler handler in _handlers) {
                count++;
                if(SendMessageToHandler(message, handler)) {
                    return true;
                }
                rejectedExceptionCount++;
            }
            if(rejectedExceptionCount == count) {
                throw new MessageRejectedException(message, "All of dispatcher's subscribers rejected Message.");
            }
            return false;
        }
    }
}
