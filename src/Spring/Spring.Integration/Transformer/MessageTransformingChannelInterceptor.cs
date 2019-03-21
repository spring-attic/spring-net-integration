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

using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// A {@link ChannelInterceptor} which invokes a {@link Transformer}
    /// when either sending-to or receiving-from a channel.
    /// </summary>
    /// <author>Jonas Partner</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MessageTransformingChannelInterceptor : ChannelInterceptorAdapter {

        private readonly ITransformer _transformer;

        private volatile bool _transformOnSend = true;


        public MessageTransformingChannelInterceptor(ITransformer transformer) {
            _transformer = transformer;
        }


        public bool TransformOnSend {
            get { return _transformOnSend; }
            set { _transformOnSend = value; }
        }

        public override IMessage PreSend(IMessage message, IMessageChannel channel) {
            if(_transformOnSend) {
                message = _transformer.Transform(message);
            }
            return message;
        }

        public override IMessage PostReceive(IMessage message, IMessageChannel channel) {
            if(!_transformOnSend) {
                message = _transformer.Transform(message);
            }
            return message;
        }
    }
}