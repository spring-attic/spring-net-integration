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

#region

using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;
using Spring.Threading.AtomicTypes;

#endregion

namespace Spring.Integration.Tests.Gateway
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class TestChannelInterceptor : ChannelInterceptorAdapter
    {
        private readonly AtomicInteger _sentCount = new AtomicInteger();

        private readonly AtomicInteger _receivedCount = new AtomicInteger();


        public int SentCount
        {
            get { return _sentCount.Value; }
        }

        public int ReceivedCount
        {
            get { return _receivedCount.Value; }
        }

        public override void PostSend(IMessage message, IMessageChannel channel, bool sent)
        {
            if (sent)
            {
                _sentCount.IncrementValueAndReturn();
            }
        }

        public override IMessage PostReceive(IMessage message, IMessageChannel channel)
        {
            if (message != null)
            {
                _receivedCount.IncrementValueAndReturn();
            }
            return message;
        }
    }
}