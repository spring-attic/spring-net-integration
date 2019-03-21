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
using System.Collections.Generic;
using Spring.Context;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Event {
    /// <summary>
    /// An inbound Channel Adapter that passes Spring
    /// {@link ApplicationEvent ApplicationEvents} within messages.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ApplicationEventInboundChannelAdapter : MessageProducerSupport, IApplicationEventListener {

        private readonly IList<Type> _eventTypes = new CopyOnWriteArrayList<Type>();

        /// <summary>
        /// Set the list of event types (classes that extend ApplicationEvent) that
        /// this adapter should send to the message channel. By default, all event
        /// types will be sent.
        /// </summary>
        public IList<Type> EventTypes {
            set {
                if(value == null || value.Count == 0)
                    throw new ArgumentException("at least one event type is required");

                lock(_eventTypes) {
                    _eventTypes.Clear();
                    foreach(Type type in value)
                        _eventTypes.Add(type);
                }
            }
        }


        public void HandleApplicationEvent(object sender, ApplicationEventArgs e) {
            if(_eventTypes == null || _eventTypes.Count == 0) {
                SendEventAsMessage(e);
                return;
            }
            foreach(Type type in _eventTypes) {
                if(type.IsAssignableFrom(e.GetType())) {
                    SendEventAsMessage(e);
                    return;
                }
            }
        }

        private bool SendEventAsMessage(ApplicationEventArgs @event) {
            return SendMessage(MessageBuilder.WithPayload(@event).Build());
        }

        protected override void DoStart() { }
        protected override void DoStop() { }
    }
}
