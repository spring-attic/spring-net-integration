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
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// A base class for {@link Transformer} implementations that modify
    /// the payload of a {@link Message}.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractPayloadTransformer<T, U> : ITransformer {

        public IMessage Transform(IMessage message) {
            try {
                U result = TransformPayload((T)message.Payload);
                return MessageBuilder.WithPayload(result).CopyHeaders(message.Headers).Build();
            }
            catch(Exception e) {
                throw new MessageTransformationException(message, "failed to transform message payload", e);
            }
        }

        protected abstract U TransformPayload(T payload);
    }
}
