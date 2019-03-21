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

namespace Spring.Integration.Channel.Interceptor {
    /// <summary>
    /// A <see cref="IChannelInterceptor"/> with no-op method implementations so that
    /// subclasses do not have to implement all of the interface's methods.
    /// </summary>
    /// <typeparam name="T">the type parameter of the <see cref="IMessageChannel"/></typeparam>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class ChannelInterceptorAdapter : IChannelInterceptor {

        public virtual IMessage PreSend(IMessage message, IMessageChannel channel) {
            return message;
        }

        public virtual void PostSend(IMessage message, IMessageChannel channel, bool sent) {
        }

        public virtual bool PreReceive(IMessageChannel channel) {
            return true;
        }

        public virtual IMessage PostReceive(IMessage message, IMessageChannel channel) {
            return message;
        }
    }
}
