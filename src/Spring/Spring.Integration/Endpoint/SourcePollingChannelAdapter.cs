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
using Spring.Integration.Message;
using Spring.Util;

namespace Spring.Integration.Endpoint {
    /// <summary>
    /// A Channel Adapter implementation for connecting a
    /// {@link MessageSource} to a {@link MessageChannel}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class SourcePollingChannelAdapter : AbstractPollingEndpoint {

        private volatile IMessageSource _source;

        private volatile IMessageChannel _outputChannel;

        private readonly MessageChannelTemplate _channelTemplate = new MessageChannelTemplate();

        /// <summary>
        /// Specify the source to be polled for Messages.
        /// </summary>
        public IMessageSource Source {
            set { _source = value; }
        }

        /// <summary>
        /// Specify the {@link MessageChannel} where Messages should be sent.
        /// </summary>
        public IMessageChannel OutputChannel {
            set { _outputChannel = value; }
        }

        /// <summary>
        /// Specify the maximum time to wait for a Message to be sent to the output channel.
        /// </summary>
        public TimeSpan SendTimeout {
            set { _channelTemplate.SendTimeout = value; }
        }

        protected override void OnInit() {
            AssertUtils.ArgumentNotNull(_source, "source must not be null");
            AssertUtils.ArgumentNotNull(_outputChannel, "outputChannel must not be null");

            if(maxMessagesPerPoll < 0) {
                // the default is 1 since a source might return
                // a non-null value every time it is invoked
                MaxMessagesPerPoll = 1;
            }
            base.OnInit();
        }

        protected override bool DoPoll() {
            IMessage message = _source.Receive();
            if(message != null) {
                return _channelTemplate.Send(message, _outputChannel);
            }
            return false;
        }
    }
}
