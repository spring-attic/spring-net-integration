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

using System.Collections;
using Spring.Integration.Core;
using Spring.Integration.Handler;

namespace Spring.Integration.Splitter {
    /// <summary>
    /// Base class for Message-splitting handlers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractMessageSplitter : AbstractReplyProducingMessageHandler {

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
            object result = SplitMessage(message);
            if(result == null) {
                return;
            }
            object correlationId = message.Headers.Id;
            if(result is ICollection) {
                ICollection items = (ICollection)result;
                int sequenceNumber = 0;
                int sequenceCount = items.Count;
                foreach(object item in items) {
                    AddReply(replyHolder, item, correlationId, ++sequenceNumber, sequenceCount);
                }
            }
            else if(result.GetType().IsArray) {
                object[] items = (object[])result;
                int sequenceNumber = 0;
                int sequenceSize = items.Length;
                foreach(object item in items) {
                    AddReply(replyHolder, item, correlationId, ++sequenceNumber, sequenceSize);
                }
            }
            else {
                AddReply(replyHolder, result, correlationId, 1, 1);
            }
        }

        private static void AddReply(ReplyMessageHolder replyHolder, object item, object correlationId, int sequenceNumber, int sequenceSize) {
            replyHolder.Add(item).SetCorrelationId(correlationId).SetSequenceNumber(sequenceNumber).SetSequenceSize(sequenceSize);
        }

        /// <summary>
        /// Subclasses must override this method to split the received Message. 
        /// </summary>
        /// <param name="message">the message to split</param>
        /// <returns>
        /// The return value may be a Collection or Array. The individual elements may
        /// be Messages, but it is not necessary. If the elements are not Messages,
        /// each will be provided as the payload of a Message. It is also acceptable
        /// to return a single Object or Message. In that case, a single reply
        /// Message will be produced.
        /// </returns>
        protected abstract object SplitMessage(IMessage message);
    }
}