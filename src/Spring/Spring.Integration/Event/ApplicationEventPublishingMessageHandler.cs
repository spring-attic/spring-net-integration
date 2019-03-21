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

using Spring.Context;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Util;

namespace Spring.Integration.Event {
    /// <summary>
    /// A {@link MessageHandler} that publishes each {@link Message} it receives as
    /// a {@link MessagingEvent}. The {@link MessagingEvent} is a subclass of
    /// Spring's {@link ApplicationEvent} used by this adapter to simply wrap the
    /// {@link Message}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ApplicationEventPublishingMessageHandler<T> : AbstractMessageHandler, IApplicationContextAware {

        private IApplicationEventPublisher applicationEventPublisher;


        public IApplicationContext ApplicationContext {
            set { applicationEventPublisher = value; }
        }

        protected override void HandleMessageInternal(IMessage message) {
            AssertUtils.ArgumentNotNull(applicationEventPublisher, "applicationEventPublisher is required");

            applicationEventPublisher.PublishEvent(this, new MessagingEventArgs(message));
        }
    }
}
