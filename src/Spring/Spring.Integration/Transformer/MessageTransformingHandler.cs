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
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Util;

namespace Spring.Integration.Transformer {
    /// <summary>
    /// A reply-producing {@link MessageHandler} that delegates to a
    /// {@link Transformer} instance to modify the received {@link Message}
    /// and sends the result to its output channel.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MessageTransformingHandler : AbstractReplyProducingMessageHandler {

        private ITransformer _transformer;

        /// <summary>
        /// Create a {@link MessageTransformingHandler} instance that delegates to
        /// the provided {@link Transformer}.
        /// </summary>
        /// <param name="transformer"></param>
        public MessageTransformingHandler(ITransformer transformer) {
            AssertUtils.ArgumentNotNull(transformer, "transformer must not be null");
            _transformer = transformer;
        }

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
            try {
                IMessage result = _transformer.Transform(message);
                if(result != null) {
                    replyHolder.Set(result);
                }
            }
            catch(Exception e) {
                if(e is MessageTransformationException) {
                    throw;
                }
                throw new MessageTransformationException(message, e);
            }
        }
    }
}
