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
using System.Collections.ObjectModel;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Handler {
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ReplyMessageHolder {

        private readonly IList<MessageBuilder> builders = new List<MessageBuilder>();

        /// <summary>
        /// clear current builder list and create one new builder from <paramref name="messageOrPayload"/>
        /// </summary>
        /// <param name="messageOrPayload">a <see cref="IMessage"/> or an arbitrary payload</param>
        /// <returns>a <see cref="MessageBuilder"/> initialized with either <see cref="IMessage"/> or a payload</returns>
        public MessageBuilder Set(object messageOrPayload) {
            return CreateAndAddBuilder(messageOrPayload, true);
        }

        /// <summary>
        /// add a new builder from <paramref name="messageOrPayload"/> to the builder list
        /// </summary>
        /// <param name="messageOrPayload">a <see cref="IMessage"/> or an arbitrary payload</param>
        /// <returns>a <see cref="MessageBuilder"/> initialized with either <see cref="IMessage"/> or a payload</returns>
        public MessageBuilder Add(object messageOrPayload) {
            return CreateAndAddBuilder(messageOrPayload, false);
        }

        /// <summary>
        /// get whether the builder list is empty
        /// </summary>
        public bool IsEmpty {
            get { return builders.Count == 0; }
        }

        /// <summary>
        /// get the builders list as <see cref="ReadOnlyCollection{T}"/>
        /// </summary>
        public IList<MessageBuilder> Builders {
            get { return new ReadOnlyCollection<MessageBuilder>(builders); }
        }

        private MessageBuilder CreateAndAddBuilder(object messageOrPayload, bool clearExistingValues) {
            MessageBuilder builder;
            if(messageOrPayload is MessageBuilder) {
                builder = (MessageBuilder)messageOrPayload;
            }
            else if(messageOrPayload is IMessage) {
                builder = MessageBuilder.FromMessage((IMessage)messageOrPayload);
            }
            else {
                builder = MessageBuilder.WithPayload(messageOrPayload);
            }
            lock(builders) {
                if(clearExistingValues) {
                    builders.Clear();
                }
                builders.Add(builder);
            }
            return builder;
        }
    }
}
