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
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Endpoint {
    /// <summary>
    /// A support class for producer endpoints that provides a setter for the
    /// output channel and a convenience method for sending Messages.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class MessageProducerSupport : AbstractEndpoint {

        private volatile IMessageChannel _outputChannel;

        private readonly MessageChannelTemplate _channelTemplate = new MessageChannelTemplate();


        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        public TimeSpan SendTimeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        protected override void OnInit() {
            AssertUtils.ArgumentNotNull(_outputChannel, "outputChannel is required");
        }

        protected bool SendMessage(IMessage message) {
            return _channelTemplate.Send(message, _outputChannel);
        }
    }
}
