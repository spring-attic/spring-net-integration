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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// Transformer that deserializes the inbound byte array payload to an object.
    /// <p>The byte array payload must be a result of serialization.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class PayloadDeserializingTransformer : AbstractPayloadTransformer<byte[], object> {

        protected override object TransformPayload(byte[] payload) {
            using(MemoryStream ms = new MemoryStream(payload)) {
                try {
                    return new BinaryFormatter().Deserialize(ms);
                }
                catch(SerializationException ex) {
                    throw new ArgumentException("Failed to deserialize payload. Is the byte array a result of Object serialization?", ex);

                }
            }
        }
    }
}
