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

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Spring.Util;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// Transformer that serializes the inbound payload into a byte array.
    /// <p>The payload instance must be Serializable.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class PayloadSerializingTransformer : AbstractPayloadTransformer<object, byte[]> {

        protected override byte[] TransformPayload(object payload) {
            
            //AssertUtils.IsTrue(payload is ISerializable, GetType().Name + " requires a Serializable payload, but received [" + payload.GetType().Name + "]");

            BinaryFormatter serializer = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, payload);
            ms.Flush();
            ms.Close();
            return ms.ToArray();
        }
    }
}
