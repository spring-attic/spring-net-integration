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
using Spring.Util;

namespace Spring.Integration.Message {

    /// <summary>
    /// The _headers for a <see cref="IMessage"/>.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public sealed class MessageBuilder {

        private readonly object _payload;

        private readonly IDictionary<string, object> _headers = new Dictionary<string, object>();

        private readonly IMessage _originalMessage;

        private volatile bool _modified;

        /// <summary>
        /// Private constructor to be invoked from the static factory methods only.
        /// </summary>
        private MessageBuilder(object payload, IMessage originalMessage) {
            AssertUtils.ArgumentNotNull(payload, "_payload must not be null");

            _payload = payload;
            _originalMessage = originalMessage;
            if(originalMessage != null) {
                CopyHeaders(originalMessage.Headers);
            }
        }

        /// <summary>
        /// Create a builder for a new <see cref="IMessage"/> instance pre-populated with
        /// all of the _headers copied from the provided message. The _payload will
        /// also be taken from the provided message.
        /// </summary>
        /// <param name="message">the Message from which the payload and all headers will be copied</param>
        /// <returns>a MessageBuilder containing the payload and the headers from <paramref name="message"/></returns>
        public static MessageBuilder FromMessage(IMessage message) {
            AssertUtils.ArgumentNotNull(message, "message must not be null");
            MessageBuilder builder = new MessageBuilder(message.Payload, message);
            return builder;
        }

        /// <summary>
        /// Create a builder for a new <see cref="IMessage"/> instance with the provided payload.
        /// </summary>
        /// <param name="payload">the payload for the new message</param>
        /// <returns>a MessageBuilder containing the payload from <paramref name="payload"/></returns>
        public static MessageBuilder WithPayload(object payload) {
            MessageBuilder builder = new MessageBuilder(payload, null);
            return builder;
        }

        /// <summary>
        /// Set the value for the given header name. If the provided value is
        /// <c>null</c>, the header will be removed.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="headerValue">the value of the header or <c>null</c></param>
        /// <returns>the instance on which <see cref="SetHeader"/> is called</returns>
        public MessageBuilder SetHeader(string headerName, object headerValue) {
            if(StringUtils.HasLength(headerName) && !IsReadOnly(headerName)) {
                _modified = true;
                if(headerValue == null) {
                    _headers.Remove(headerName);
                }
                else {
                    _headers[headerName] = headerValue;
                }
            }

            return this;
        }

        /// <summary>
        /// Set the value for the given header name only if the header name
        /// is not already associated with a value.
        /// </summary>
        /// <param name="headerName">the name of the header</param>
        /// <param name="headerValue">the value of the header or <c>null</c></param>
        /// <returns>the instance on which <see cref="SetHeaderIfAbsent"/> is called</returns>
        public MessageBuilder SetHeaderIfAbsent(string headerName, object headerValue) {
            if(!_headers.ContainsKey(headerName)) {
                SetHeader(headerName, headerValue);
            }
            return this;
        }

        /// <summary>
        /// Remove the value for the given header name.
        /// </summary>
        /// <param name="headerName">the name of the header which should be removed</param>
        public MessageBuilder RemoveHeader(string headerName) {
            if(StringUtils.HasLength(headerName)) {
                _modified = true;
                _headers.Remove(headerName);
            }
            return this;
        }

        /// <summary>
        /// Copy the name-value pairs from the provided Map. This operation will
        /// overwrite any existing values. Use <see cref="CopyHeadersIfAbsent"/> 
        /// to avoid overwriting values. Note that the 'id' and 'timestamp' header
        /// values will never be overwritten.
        /// <see cref="MessageHeaders.ID"/>
        /// <see cref="MessageHeaders.TIMESTAMP"/>
        /// </summary>
        /// <param name="headersToCopy">the headers to be copied to the current instance</param>
        /// <returns>the instance on which <see cref="CopyHeaders"/> is called</returns>
        public MessageBuilder CopyHeaders(IDictionary<string, object> headersToCopy) {
            foreach(string key in headersToCopy.Keys) {
                SetHeader(key, headersToCopy[key]);
            }

            return this;
        }

        /// <summary>
        //	 Copy the name-value pairs from the provided Map. This operation will
        //	 <em>not</em> overwrite any existing values.
        /// </summary>
        /// <param name="headersToCopy">the headers to be copied to the current instance</param>
        /// <returns>the instance on which <see cref="CopyHeadersIfAbsent"/> is called</returns>
        public MessageBuilder CopyHeadersIfAbsent(IDictionary<string, object> headersToCopy) {
            foreach(string key in headersToCopy.Keys) {
                SetHeaderIfAbsent(key, headersToCopy[key]);
            }

            return this;
        }

        ///// <summary>
        ///// set the expiration date
        ///// </summary>
        ///// <param name="expirationDate">the expiration date in ticks</param>
        ///// <returns>the current instance</returns>
        //public MessageBuilder SetExpirationDate(long expirationDate) {
        //    return SetHeader(MessageHeaders.EXPIRATION_DATE, expirationDate);
        //}

        /// <summary>
        /// set the expiration date
        /// </summary>
        /// <param name="expirationDate">the expiration date as date time</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetExpirationDate(DateTime expirationDate) {
            if(expirationDate != default(DateTime)) {
                return SetHeader(MessageHeaders.EXPIRATION_DATE, expirationDate);
            }
            return SetHeader(MessageHeaders.EXPIRATION_DATE, null);
        }

        /// <summary>
        /// set the correlation id
        /// </summary>
        /// <param name="correlationId">the correlation id</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetCorrelationId(object correlationId) {
            return SetHeader(MessageHeaders.CORRELATION_ID, correlationId);
        }

        /// <summary>
        /// set the reply channel
        /// </summary>
        /// <param name="replyChannel">the reply channel</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetReplyChannel(IMessageChannel replyChannel) {
            return SetHeader(MessageHeaders.REPLY_CHANNEL, replyChannel);
        }

        /// <summary>
        /// set the name of the reply channel
        /// </summary>
        /// <param name="replyChannelName">the name of the reply channel</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetReplyChannelName(string replyChannelName) {
            return SetHeader(MessageHeaders.REPLY_CHANNEL, replyChannelName);
        }

        /// <summary>
        /// set the error channel
        /// </summary>
        /// <param name="errorChannel">the erroe channel</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetErrorChannel(IMessageChannel errorChannel) {
            return SetHeader(MessageHeaders.ERROR_CHANNEL, errorChannel);
        }

        /// <summary>
        /// set the name of the error channel
        /// </summary>
        /// <param name="errorChannelName">the name of the error channel</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetErrorChannelName(string errorChannelName) {
            return SetHeader(MessageHeaders.ERROR_CHANNEL, errorChannelName);
        }

        /// <summary>
        /// set the sequence number
        /// </summary>
        /// <param name="sequenceNumber">the sequence number</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetSequenceNumber(int sequenceNumber) {
            return SetHeader(MessageHeaders.SEQUENCE_NUMBER, sequenceNumber);
        }

        /// <summary>
        /// set the sequence size
        /// </summary>
        /// <param name="sequenceSize">the sequence size</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetSequenceSize(int sequenceSize) {
            return SetHeader(MessageHeaders.SEQUENCE_SIZE, sequenceSize);
        }

        /// <summary>
        /// set the priority
        /// </summary>
        /// <param name="priority">the message priority</param>
        /// <returns>the current instance</returns>
        public MessageBuilder SetPriority(MessagePriority priority) {
            return SetHeader(MessageHeaders.PRIORITY, priority);
        }

        /// <summary>
        /// build a new message
        /// </summary>
        /// <returns></returns>
        public IMessage Build() {
            if(!_modified && _originalMessage != null) {
                return _originalMessage;
            }
            return new Message(_payload, _headers);
        }

        /// <summary>
        /// test if the value for <paramref name="key"/> is readonly
        /// </summary>
        /// <param name="key">the key to test</param>
        /// <returns><c>true</c> if <paramref name="key"/>is readonly <c>false</c> otherwise</returns>
        private static bool IsReadOnly(string key) {
            return (key.Equals(MessageHeaders.ID) || key.Equals(MessageHeaders.TIMESTAMP));
        }
    }
}
