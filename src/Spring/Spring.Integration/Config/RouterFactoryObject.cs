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
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Router;
using Spring.Util;

namespace Spring.Integration.Config {
    /// <summary>
    /// Factory bean for creating a Message Router.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class RouterFactoryObject : AbstractMessageHandlerFactoryObject {

        private volatile IChannelResolver _channelResolver;

        private volatile IMessageChannel _defaultOutputChannel;


        public IChannelResolver ChannelResolver {
            set { _channelResolver = value; }
        }

        public IMessageChannel DefaultOutputChannel {
            set { _defaultOutputChannel = value; }
        }

        protected override IMessageHandler CreateHandler(object targetObject, string targetMethodName) {
            AssertUtils.ArgumentNotNull(targetObject, "target object must not be null");
            AbstractMessageRouter router = CreateRouter(targetObject, targetMethodName);
            if(_defaultOutputChannel != null) {
                router.DefaultOutputChannel = _defaultOutputChannel;
            }
            return router;
        }

        private AbstractMessageRouter CreateRouter(object targetObject, string targetMethodName) {
            if(targetObject is AbstractMessageRouter) {
                AssertUtils.IsTrue(!StringUtils.HasText(targetMethodName),
                        "target method should not be provided when the target " +
                        "object is an implementation of AbstractMessageRouter");
                return (AbstractMessageRouter)targetObject;
            }
            MethodInvokingRouter router = (StringUtils.HasText(targetMethodName))
                    ? new MethodInvokingRouter(targetObject, targetMethodName)
                    : new MethodInvokingRouter(targetObject);
            if(_channelResolver != null) {
                router.ChannelResolver = _channelResolver;
            }
            return router;
        }
    }
}
