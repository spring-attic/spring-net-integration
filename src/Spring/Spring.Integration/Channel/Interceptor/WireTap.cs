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
using Common.Logging;
using Spring.Context;
using Spring.Integration.Core;
using Spring.Integration.Selector;
using Spring.Util;

namespace Spring.Integration.Channel.Interceptor {
    /// <summary>
    /// A <see cref="IChannelInterceptor"/> that publishes a copy of the intercepted message
    /// to a secondary target while still sending the original message to the main channel.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class WireTap : ChannelInterceptorAdapter, ILifecycle {

        private static readonly ILog logger = LogManager.GetLogger(typeof(WireTap));

        private readonly IMessageChannel _channel;

        private /*volatile*/ TimeSpan _timeout;

        private readonly IMessageSelector _selector;

        private volatile bool _running = true;


        /// <summary>
        /// Create a new wire tap with <em>no</em> <see cref="IMessageSelector"/>
        /// </summary>
        /// <param name="channel">the <see cref="IMessageChannel"/> to which intercepted messages will be sent</param>
        public WireTap(IMessageChannel channel)
            : this(channel, null) {
        }

        /// <summary>
        /// Create a new wire tap with the provided <see cref="IMessageSelector"/>.
        /// </summary>
        /// <param name="channel">the <see cref="IMessageChannel"/> to which intercepted messages will be sent</param>
        /// <param name="selector">the <see cref="IMessageSelector"/> that must accept a message for it to be sent to the intercepting channel</param>
        public WireTap(IMessageChannel channel, IMessageSelector selector) {
            AssertUtils.ArgumentNotNull(channel, "_channel", "_channel must not be null");
            _channel = channel;
            _selector = selector;
        }

        /// <summary>
        /// Specify the _timeout value for sending to the intercepting target. Note
        /// that this value will only apply if the target is a {@link BlockingTarget}.
        /// The default value is 0.
        /// </summary>
        public TimeSpan Timeout {
            set {
                lock(this) {
                    _timeout = value;
                }
            }
        }

        #region ILifecycle Members

        /// <summary>
        /// Check whether the wire tap is currently running.
        /// </summary>
        public bool IsRunning {
            get { return _running; }
        }

        /// <summary>
        /// Restart the wire tap if it has been stopped. It is running by default.
        /// </summary>
        public void Start() {
            _running = true;
        }

        /// <summary>
        /// Stop the wire tap. To restart, invoke <see cref="Start"/>.
        /// </summary>
        public void Stop() {
            _running = false;
        }

        #endregion

        /// <summary>
        /// Intercept the Message and, <em>if accepted</em> by the <see cref="IMessageSelector"/>,
        /// send it to the secondary target. If this wire tap's <see cref="IMessageSelector"/> is
        /// <code>null</code>, it will accept all messages.
        /// </summary>
        /// <param name="message">the message to intercept</param>
        /// <param name="channel">the channel to send to if accepted</param>
        /// <returns></returns>
        public override IMessage PreSend(IMessage message, IMessageChannel channel) {
            if(_running && (_selector == null || _selector.Accept(message))) {
                bool sent = _timeout == TimeSpan.Zero ? _channel.Send(message, _timeout) : _channel.Send(message);

                #region logging
                if(!sent && logger.IsWarnEnabled) {
                    logger.Warn("failed to send message to WireTap _channel '" + _channel + "'");
                }
                #endregion
            }
            return message;
        }

    }
}
