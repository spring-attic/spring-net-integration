#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using Spring.Integration.Endpoint;
using Spring.Messaging.Nms.Listener;
using Spring.Util;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// A message-driven endpoint that receive NMS messages, converts them into
    /// Spring Integration Messages, and then sends the result to a channel.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class NmsMessageDrivenEndpoint : AbstractEndpoint, IDisposable
    {

        private readonly AbstractMessageListenerContainer listenerContainer;

        private readonly ChannelPublishingJmsMessageListener listener;

        public NmsMessageDrivenEndpoint(AbstractMessageListenerContainer listenerContainer, ChannelPublishingJmsMessageListener listener)
        {
            AssertUtils.ArgumentNotNull(listenerContainer, "listener container must not be null");
            AssertUtils.ArgumentNotNull(listener, "listener must not be null");
            listenerContainer.MessageListener = listener;
            this.listenerContainer = listenerContainer;
            this.listener = listener;
        }


        protected override void OnInit()
        {
            listener.AfterPropertiesSet();
            if (!listenerContainer.Active)
            {
                listenerContainer.AfterPropertiesSet();
            }
        }

        #region Overrides of AbstractEndpoint

        protected override void DoStart()
        {
            if (!listenerContainer.IsRunning)
            {
                listenerContainer.Start();
            }
        }

        protected override void DoStop()
        {
            listenerContainer.Stop();            
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (IsRunning)
            {
                Stop();
            }
            listenerContainer.Dispose();
        }

        #endregion
    }

}