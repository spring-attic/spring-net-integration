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
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Endpoint {
    /// <summary>
    /// Message Endpoint that connects any {@link MessageHandler} implementation
    /// to a {@link PollableChannel}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class PollingConsumer : AbstractPollingEndpoint {

        private readonly IPollableChannel _inputChannel;

        private readonly IMessageHandler _handler;

        private TimeSpan _receiveTimeout = TimeSpan.FromMilliseconds(1000);


        public PollingConsumer(IPollableChannel inputChannel, IMessageHandler handler) {
            AssertUtils.ArgumentNotNull(inputChannel, "inputChannel");
            AssertUtils.ArgumentNotNull(handler, "handler");
            _inputChannel = inputChannel;
            _handler = handler;
        }


        public TimeSpan ReceiveTimeout {
            set {
                lock(this) {
                    _receiveTimeout = value;
                }
            }
        }

        protected override bool DoPoll() {
            IMessage message = _receiveTimeout.TotalMilliseconds >= 0 ? _inputChannel.Receive(_receiveTimeout) : _inputChannel.Receive();
            if(message == null) {
                return false;
            }
            _handler.HandleMessage(message);
            return true;
        }
    }
}
