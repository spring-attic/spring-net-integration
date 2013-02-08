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

namespace Spring.Integration.Gateway {

    /// <summary>
    /// An implementation of the {@link InboundMessageMapper} and
    /// {@link OutboundMessageMapper} strategy interfaces that maps directly to and
    /// from the Message payload instance.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class SimpleMessageMapper : IInboundMessageMapper, IOutboundMessageMapper {

        /**
         * Returns the Message payload (or null if the Message is null).
         */
        public object FromMessage(IMessage message) {
            if(message == null || message.Payload == null) {
                return null;
            }
            return message.Payload;
        }

        /**
         * Returns a Message with the given object as its payload, unless the
         * object is already a Message in which case it will be returned as-is.
         * If the object is null, the returned Message will also be null.
         */
        public IMessage ToMessage(object obj) {
            if(obj == null) {
                return null;
            }
            if(obj is IMessage) {
                return (IMessage)obj;
            }
            return MessageBuilder.WithPayload(obj).Build();
        }
    }
}
