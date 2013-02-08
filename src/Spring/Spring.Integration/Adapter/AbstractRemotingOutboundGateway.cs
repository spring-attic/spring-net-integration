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

using System.Runtime.Remoting;
using System.Runtime.Serialization;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;

namespace Spring.Integration.Adapter {
    /// <summary>
    /// A base class for outbound Messaging Gateways that use url-based remoting.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class AbstractRemotingOutboundGateway : AbstractReplyProducingMessageHandler {

        private readonly IRemoteMessageHandler _handlerProxy;


        public AbstractRemotingOutboundGateway(string url) {
            _handlerProxy = CreateHandlerProxy(url);
        }

        public IMessageChannel ReplyChannel {
            set { OutputChannel = value; }
        }

        /**
         * Subclasses must implement this method. It will be invoked from the constructor.
         */
        protected abstract IRemoteMessageHandler CreateHandlerProxy(string url);


        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
            if(!(message.Payload is ISerializable)) {
                throw new MessageHandlingException(message, GetType().Name + " expects a Serializable payload type " + "but encountered [" + message.Payload.GetType().Name + "]");
            }
            IMessage requestMessage = MessageBuilder.FromMessage(message).Build();
            try {
                IMessage reply = _handlerProxy.Handle(requestMessage);
                if(reply != null) {
                    replyHolder.Set(reply);
                }
            }
            catch(RemotingException e) {
                throw new MessageHandlingException(message, "unable to handle message remotely", e);
            }
        }
    }
}
