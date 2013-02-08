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
using System.Collections;
using System.Collections.Generic;
using Common.Logging;
using Spring.Integration.Core;
using Spring.Objects.Factory;
using Spring.Threading.Collections.Generic;

namespace Spring.Integration.Channel {
    /// <summary>
    /// Base class for <see cref="IMessage"/> implementations providing common
    /// properties such as the channel name. Also provides the common functionality
    /// for sending and receiving <see cref="IMessage"/> Messages including the invocation
    /// of any <see cref="IChannelInterceptor"/> ChannelInterceptors.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public abstract class AbstractMessageChannel : IMessageChannel, IObjectNameAware {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AbstractMessageChannel));

        private volatile string _name;

        private readonly ChannelInterceptorList _interceptors = new ChannelInterceptorList();

        #region IObjectNameAware Members

        /// <summary>
        /// Set the name of this channel. This will be invoked automatically whenever
        /// the channel is configured explicitly with a bean definition.
        /// </summary>
        public string ObjectName {
            set { _name = value; }
        }

        #endregion

        #region IMessageChannel Members

        /// <summary>
        /// Return the name of this channel.
        /// </summary>
        public string Name {
            get { return _name; }
        }

        /// Send a message on this channel. If the channel is at capacity, this
        /// method will block until either space becomes available or the sending
        /// thread is interrupted.
        /// </summary>
        /// <param name="message">the Message to send</param>
        /// <returns>
        /// <code>true</code> if the message is sent successfully or
        /// <code>false</code> if the sending thread is interrupted.
        /// </returns>
        public bool Send(IMessage message) {
            return Send(message, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Send a message on this channel. If the channel is at capacity, this
        /// method will block until either the timeout occurs or the sending thread
        /// is interrupted. If the specified timeout is 0, the method will return
        /// immediately. If less than zero, it will block indefinitely <see cref="Send(IMessage)"/> 
        /// </summary>
        /// <param name="message">the Message to send</param>
        /// <param name="timeout">the timeout in milliseconds</param>
        /// <returns>
        /// <code>true</code> if the message is sent successfully,
        /// <code>false</code> if the message cannot be sent within the allotted
        /// time or the sending thread is interrupted.
        /// </returns>
        public bool Send(IMessage message, TimeSpan timeout) {
            message = _interceptors.PreSend(message, this);
            if (message == null) {
                return false;
            }
            bool sent = DoSend(message, timeout);
            _interceptors.PostSend(message, this, sent);
            return sent;
        }

        #endregion

        /// <summary>
        /// <summary>
        /// Set the list of channel interceptors. This will clear any existing
        /// interceptors.
        /// </summary>    
        public void SetInterceptors(IList<IChannelInterceptor>  interceptors) { 
                _interceptors.Set(interceptors); 
        }

        /// <summary>
        /// Add a channel interceptor to the end of the list.
        /// </summary>
        public void AddInterceptor(IChannelInterceptor interceptor) {
            _interceptors.Add(interceptor);
        }

        /// <summary>
        /// Exposes the interceptor list for subclasses.
        /// </summary>    
        protected ChannelInterceptorList Interceptors {
            get { return _interceptors; }
        }
 
        /// <summary>
        /// Subclasses must implement this method. A non-negative timeout indicates
        /// how long to wait if the channel is at capacity (if the value is 0, it
        /// must return immediately with or without success). A negative timeout
        /// value indicates that the method should block until either the message is
        /// accepted or the blocking thread is interrupted.
        /// </summary>
        protected abstract bool DoSend(IMessage message, TimeSpan timeout);

        #region object overrides
        public override string ToString() {
                return (_name != null) ? _name : base.ToString();
        }
        #endregion

        /// <summary>
        /// A convenience wrapper class for the list of ChannelInterceptors.
        /// </summary>
        protected class ChannelInterceptorList : IList {
            private readonly CopyOnWriteArrayList<IChannelInterceptor> _interceptors = new CopyOnWriteArrayList<IChannelInterceptor>();

            public void Set(IList<IChannelInterceptor> interceptors) {
                lock(_interceptors) {
                    _interceptors.Clear();
                    _interceptors.AddAll(interceptors);
                }
            }

            public int Add(IChannelInterceptor interceptor) {
                return _interceptors.Add(interceptor);
            }

            public IMessage PreSend(IMessage message, IMessageChannel channel) {
                #region logging
                if(_logger.IsDebugEnabled) {
                    _logger.Debug("preSend on channel '" + channel + "', message: " + message);
                }
                #endregion
                foreach(IChannelInterceptor interceptor in _interceptors) {
                    message = interceptor.PreSend(message, channel);
                    if(message == null) {
                        return null;
                    }
                }
                return message;
            }

            public void PostSend(IMessage message, IMessageChannel channel, bool sent) {
                #region logging
                if(_logger.IsDebugEnabled) {
                    _logger.Debug("postSend (sent=" + sent + ") on channel '" + channel + "', message: " + message);
                }
                #endregion
                foreach(IChannelInterceptor interceptor in _interceptors) {
                    interceptor.PostSend(message, channel, sent);
                }
            }

            public bool PreReceive(IMessageChannel channel) {
                #region logging
                if(_logger.IsTraceEnabled) {
                    _logger.Trace("preReceive on channel '" + channel + "'");
                }
                #endregion
                foreach(IChannelInterceptor interceptor in _interceptors) {
                    if(!interceptor.PreReceive(channel)) {
                        return false;
                    }
                }
                return true;
            }

            public IMessage PostReceive(IMessage message, IMessageChannel channel) {
                #region logging
                if(message != null && _logger.IsDebugEnabled) {
                    _logger.Debug("postReceive on channel '" + channel + "', message: " + message);
                }
                else if(_logger.IsTraceEnabled) {
                    _logger.Trace("postReceive on channel '" + channel + "', message is null");
                }
                #endregion
                foreach(IChannelInterceptor interceptor in _interceptors) {
                    message = interceptor.PostReceive(message, channel);
                    if(message == null) {
                        return null;
                    }
                }
                return message;
            }

            #region IList Members

            public int Add(object value) {
                return Add((IChannelInterceptor) value);
            }

            public void Clear() {
                throw new NotImplementedException();
            }

            public bool Contains(object value) {
                throw new NotImplementedException();
            }

            public int IndexOf(object value) {
                throw new NotImplementedException();
            }

            public void Insert(int index, object value) {
                throw new NotImplementedException();
            }

            public bool IsFixedSize {
                get { return false; }
            }

            public bool IsReadOnly {
                get { return false; }
            }

            public void Remove(object value) {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index) {
                throw new NotImplementedException();
            }

            public object this[int index] {
                get {
                    throw new NotImplementedException();
                }
                set {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region ICollection Members

            public void CopyTo(Array array, int index) {
                throw new NotImplementedException();
            }

            public int Count {
                get { throw new NotImplementedException(); }
            }

            public bool IsSynchronized {
                get { throw new NotImplementedException(); }
            }

            public object SyncRoot {
                get { throw new NotImplementedException(); }
            }

            #endregion

            #region IEnumerable Members

            public IEnumerator GetEnumerator() {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}
