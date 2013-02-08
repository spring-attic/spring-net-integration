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

using Spring.Integration.Channel;
using Spring.Integration.Message;
using Spring.Util;

namespace Spring.Integration.Endpoint {
    /// <summary>
    /// Message Endpoint that connects any {@link MessageHandler} implementation
    /// to a {@link SubscribableChannel}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class EventDrivenConsumer : AbstractEndpoint {

        private readonly ISubscribableChannel _inputChannel;

        private  readonly IMessageHandler _handler;


        public EventDrivenConsumer(ISubscribableChannel inputChannel, IMessageHandler handler) {
            AssertUtils.ArgumentNotNull(inputChannel, "inputChannel must not be null");
            AssertUtils.ArgumentNotNull(handler, "handler must not be null");
            _inputChannel = inputChannel;
            _handler = handler;
        }


        // guarded by super#lifecycleLock
        protected override void DoStart() {
            _inputChannel.Subscribe(_handler);
        }

        // guarded by super#lifecycleLock
        protected override void DoStop() {
            _inputChannel.Unsubscribe(_handler);
        }
    }
}
