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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Spring.Integration.Util;
using Spring.Util;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// A Transformer that adds statically configured header values to a Message.
    /// Accepts the boolean 'overwrite' property that specifies whether values
    /// should be overwritten. By default, any existing header values for
    /// a given key, will <em>not</em> be replaced.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class HeaderEnricher : AbstractHeaderTransformer {

        private readonly IDictionary<string, object> _headersToAdd;

        private volatile bool _overwrite;

        /// <summary>
        /// Create a HeaderEnricher with the given map of headers.
        /// </summary>
        public HeaderEnricher(IDictionary<string, object> headersToAdd) {
            AssertUtils.ArgumentNotNull(headersToAdd, "headersToAdd must not be null");
            _headersToAdd = headersToAdd;
        }

        /// <summary>
        /// Create a HeaderEnricher with the given map of headers.
        /// </summary>
        public HeaderEnricher(IDictionary headersToAdd) {
            AssertUtils.ArgumentNotNull(headersToAdd, "headersToAdd must not be null");
            _headersToAdd = new Dictionary<string, object>();
            foreach(object obj in headersToAdd.Keys) {
                _headersToAdd.Add((string)obj, headersToAdd[obj]);
            }
        }

        /// <summary>
        /// set overwrite flag
        /// </summary>
        public bool Overwrite {
            set { _overwrite = value; }
        }

        protected override void TransformHeaders(IDictionary<string, object> headers) {
            foreach(KeyValuePair<string, object> entry in _headersToAdd) {
                string key = entry.Key;
                if(_overwrite || DictionaryUtils.Get(headers, key) == null) {
                    DictionaryUtils.Put(headers, key, entry.Value);
                }
            }
        }
    }
}
