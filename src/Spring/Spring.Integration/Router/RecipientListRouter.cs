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

using System;
using System.Collections.Generic;
using Spring.Integration.Core;
using Spring.Objects.Factory;

namespace Spring.Integration.Router {
    /// <summary>
    /// A Message Router that sends Messages to a statically configured list of
    /// recipients. The recipients are provided as a list of {@link MessageChannel}
    /// instances. For dynamic recipient lists, consider instead using the @Router
    /// annotation or extending {@link AbstractChannelNameResolvingMessageRouter}. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class RecipientListRouter : AbstractMessageRouter, IInitializingObject {

        private volatile IList<IMessageChannel> _channels;

        public IList<IMessageChannel> Channels {
            set { _channels = value; }
        }

        public void AfterPropertiesSet() {
            if(_channels == null || _channels.Count == 0)
                throw new ArgumentException("a non-empty channel list is required");
        }

        protected override ICollection<IMessageChannel> DetermineTargetChannels(IMessage message) {
            return _channels;
        }
    }
}
