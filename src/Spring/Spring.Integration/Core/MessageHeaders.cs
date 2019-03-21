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
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Spring.Integration.Core {
    /// <summary>
    /// The headers for a  <see cref="IMessage"/>.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [Serializable]
    public sealed class MessageHeaders : IDictionary<string, object> { 

        //private static ILog _logger = LogManager.GetLogger(typeof(MessageHeaders));

        public const string PREFIX = "springintegration_";
        /// <summary>
        /// Name of the header used to identify the message as a Spring Integration Message
        /// </summary>
        public const string ID = PREFIX + "id";

        /// <summary>
        /// Name of the header used for the SI timestamp
        /// </summary>
        public const string TIMESTAMP = PREFIX + "timestamp";

        /// <summary>
        /// Name of the header used for the SI correlation id
        /// </summary>
        public const string CORRELATION_ID = PREFIX + "correlationId";

        /// <summary>
        /// Name of the header used for the SI replay channel
        /// </summary>
        public const string REPLY_CHANNEL = PREFIX + "replyChannel";

        /// <summary>
        /// Name of the header used for the SI error channel
        /// </summary>
        public const string ERROR_CHANNEL = PREFIX + "errorChannel";

        /// <summary>
        /// Name of the header used for the SI message expiration date
        /// </summary>
        public const string EXPIRATION_DATE = PREFIX + "expirationDate";

        /// <summary>
        /// Name of the header used for the SI message priority
        /// </summary>
        public const string PRIORITY = PREFIX + "priority";

        /// <summary>
        /// Name of the header used for the SI sequence number
        /// </summary>
        public const string SEQUENCE_NUMBER = PREFIX + "sequenceNumber";

        /// <summary>
        /// Name of the header used for the SI sequence size
        /// </summary>
        public const string SEQUENCE_SIZE = PREFIX + "sequenceSize";


        private readonly IDictionary<string, object> _headers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHeaders"/> class from <paramref name="headers"/>
        /// </summary>
        /// <remarks>Add a new ID and TIMESTAMP to the MessageHeader if not present
        /// in the passed in dictionary.</remarks>
        /// <param name="headers">the dictionary which is the base of the new one</param>
        public MessageHeaders(IDictionary<string, object> headers) {
            _headers = (headers != null)
                    ? new Dictionary<string, object>(headers)
                    : new Dictionary<string, object>();            
            if (!_headers.ContainsKey(ID))
            {
                _headers[ID] = Guid.NewGuid();
            }
            if (!_headers.ContainsKey(TIMESTAMP))
            {
                _headers[TIMESTAMP] = DateTime.UtcNow.Ticks;
            }
        }

        /// <summary>
        /// get the Id 
        /// </summary>
        public object Id {
            get { return Get(ID); }
        }

        /// <summary>
        /// get the timestamp
        /// </summary>
        public long Timestamp {
            get { return Get<long>(TIMESTAMP); }
        }

        /// <summary>
        /// get the expiration date
        /// </summary>
        public DateTime ExpirationDate {
            get { return Get<DateTime>(EXPIRATION_DATE); }
        }

        /// <summary>
        /// test whether an expiration date entry is available
        /// </summary>
        public bool HasExpirationDate {
            get {
                return _headers.ContainsKey(EXPIRATION_DATE);
            }
        }


        /// <summary>
        /// get the correlation id or null
        /// </summary>
        public object CorrelationId {
            get { return Get(CORRELATION_ID); }
        }

        /// <summary>
        /// get the reply channel or null
        /// </summary>
        public object ReplyChannel {
            get { return Get(REPLY_CHANNEL); }
        }

        /// <summary>
        /// get the error channel or null
        /// </summary>
        public object ErrorChannel {
            get { return Get(ERROR_CHANNEL); }
        }

        /// <summary>
        /// get the sequence number or zero
        /// </summary>
        public int SequenceNumber {
            get { return Get<int>(SEQUENCE_NUMBER); }
        }
        
        /// <summary>
        /// get the sequence number or zero
        /// </summary>
        public int SequenceSize {
            get { return Get<int>(SEQUENCE_SIZE); }
        }
        
        /// <summary>
        /// get the priority of the message or null
        /// </summary>
        public MessagePriority Priority {
            get { return Get<MessagePriority>(PRIORITY); }
        }

        /// <summary>
        /// try to get the value corresponding to key. 
        /// </summary>
        /// <typeparam name="T">the expected type of the value</typeparam>
        /// <param name="key">the key</param>
        /// <returns>the value of the dictionary item if key exists, otherwise default(<typeparamref name="T"/>)</returns>
        /// <exception cref="ArgumentException">if value is not assignable to <typeparamref name="T"/></exception>
        public T Get<T>(string key) {
            object value;
            if (!_headers.TryGetValue(key, out value))
                return default(T);

            if (!typeof (T).IsAssignableFrom(value.GetType())) {
                throw new ArgumentException("Incorrect type specified for header '" + key
                                            + "'. Expected [" + typeof (T) + "] but actual type is [" + value.GetType() +
                                            "]");
            }
            return (T) value;
        }

        /// <summary>
        /// try to get the value corresponding to key. 
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the value of the dictionary item if key exists, otherwise <c>null</c></returns>
        public object Get(string key) {
            object value;
            return !_headers.TryGetValue(key, out value) ? null : value;
        }

        #region object overrides

        public override int GetHashCode() {
            return _headers.GetHashCode();
        }


        public override bool Equals(object obj) {
            if (this == obj) {
                return true;
            }

            if (obj != null && obj.GetType() == typeof (MessageHeaders)) {
                MessageHeaders other = (MessageHeaders) obj;
                return _headers.Equals(other._headers);
            }
            return false;
        }

        public override string ToString() {
            return _headers.ToString();
        }

        #endregion

        #region IDictionary<string,object> Members

        public void Add(string key, object value) {
            throw new NotSupportedException("MessageHeader is immutable");
        }

        public bool ContainsKey(string key) {
            return _headers.ContainsKey(key);
        }

        public ICollection<string> Keys {
            get { return new ReadOnlyCollection<string>(new List<string>(_headers.Keys)); }
        }

        public bool Remove(string key) {
            throw new NotSupportedException("MessageHeader is immutable");
        }

        public bool TryGetValue(string key, out object value) {
            return _headers.TryGetValue(key, out value);
        }

        public ICollection<object> Values {
            get { return new ReadOnlyCollection<object>(new List<object>(_headers.Values)); }
        }

        public object this[string key] {
            get {
                return _headers[key];
            }
            set {
                throw new NotSupportedException("MessageHeader is immutable");
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<string, object> item) {
            throw new NotSupportedException("MessageHeader is immutable");
        }

        public void Clear() {
            throw new NotSupportedException("MessageHeader is immutable");
        }

        public bool Contains(KeyValuePair<string, object> item) {
            return _headers.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) {
            _headers.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _headers.Count; }
        }

        public bool IsReadOnly {
            get { return true;  }
        }

        public bool Remove(KeyValuePair<string, object> item) {
            throw new NotSupportedException("MessageHeader is immutable");
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            return _headers.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return _headers.GetEnumerator();
        }

        #endregion

        //TODO implement ISerializable to filter out non-serializable values in the header.

    }
}
