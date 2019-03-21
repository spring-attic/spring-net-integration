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

using System.Collections.Generic;
namespace Spring.Integration.Util {
    /// <summary>
    /// utility class for <see cref="Dictionary{TKey,TValue}"/>
    /// </summary>
    /// <author>Andreas D�hring (.NET)</author>
    public class DictionaryUtils {
        /// <summary>
        /// Associates the specified value with the specified key in this map. If the map previously contained a mapping for this key, the old value is replaced. 
        /// </summary>
        public static void Put<TKey, TValue>(IDictionary<TKey,TValue> dictionary, TKey key, TValue value) {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Returns the value to which the specified key is mapped or null if the dictionary contains no mapping for this key. 
        /// </summary>
        public static TValue Get<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }
    }
}