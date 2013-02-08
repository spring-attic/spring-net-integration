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

#region

using System.Collections.Generic;
using System.Text;
using Spring.Integration.Aggregator;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class TestAggregatorObject
    {
        // TODO private final ConcurrentMap<Object, Message<?>> aggregatedMessages = new ConcurrentHashMap<object, IMessage>();
        private readonly IDictionary<object, IMessage> aggregatedMessages = new Dictionary<object, IMessage>();


        [Aggregator]
        public IMessage CreateSingleMessageFromGroup(IList<IMessage> messages)
        {
            List<IMessage> sortableList = new List<IMessage>(messages);

            sortableList.Sort(new MessageSequenceComparator());
            StringBuilder buffer = new StringBuilder();
            object correlationId = null;
            foreach (IMessage message in sortableList)
            {
                buffer.Append(message.Payload.ToString());
                if (null == correlationId)
                {
                    correlationId = message.Headers.CorrelationId;
                }
            }
            IMessage returnedMessage = new StringMessage(buffer.ToString());
            aggregatedMessages.Add(correlationId, returnedMessage);
            return returnedMessage;
        }

        public IDictionary<object, IMessage> AggregatedMessages
        {
            get { return aggregatedMessages; }
        }
    }
}