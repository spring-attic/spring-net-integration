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
using Spring.Util;

namespace Spring.Integration.Gateway {

    /// <summary>
    /// An implementation of {@link AbstractMessagingGateway} that delegates to
    /// an {@link InboundMessageMapper} and {@link OutboundMessageMapper}. The
    /// default implementation for both is {@link SimpleMessageMapper}.
    /// 
    /// @see InboundMessageMapper
    /// @see OutboundMessageMapper
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class SimpleMessagingGateway : AbstractMessagingGateway {

        private readonly IInboundMessageMapper _inboundMapper;

        private readonly IOutboundMessageMapper _outboundMapper;


        public SimpleMessagingGateway() {
            SimpleMessageMapper mapper = new SimpleMessageMapper();
            _inboundMapper = mapper;
            _outboundMapper = mapper;
        }

        public SimpleMessagingGateway(IInboundMessageMapper inboundMapper, IOutboundMessageMapper outboundMapper) {
            AssertUtils.ArgumentNotNull(inboundMapper, "InboundMessageMapper must not be null");
            AssertUtils.ArgumentNotNull(outboundMapper, "OutboundMessageMapper must not be null");
            _inboundMapper = inboundMapper;
            _outboundMapper = outboundMapper;
        }


        protected override object FromMessage(IMessage message) {
            return _outboundMapper.FromMessage(message);
        }

        protected override IMessage ToMessage(object obj) {
            return _inboundMapper.ToMessage(obj);
        }
    }
}
