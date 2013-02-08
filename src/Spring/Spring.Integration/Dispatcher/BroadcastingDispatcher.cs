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

using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Threading;

namespace Spring.Integration.Dispatcher {
    /// <summary>
    /// A broadcasting dispatcher implementation. It makes a best effort to
    /// send the message to each of its handlers. If it fails to send to any
    /// one handler, it will log a warn-level message but continue to send
    /// to the other handlers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public class BroadcastingDispatcher : AbstractDispatcher {

        private volatile bool _applySequence;

        public BroadcastingDispatcher(IExecutor taskExecutor)
        {
            TaskExecutor = taskExecutor;
        }

        /// <summary>
        /// Specify whether to apply sequence numbers to the messages
        /// prior to sending to the handlers. By default, sequence
        /// numbers will <em>not</em> be applied
        /// </summary>
        public bool ApplySequence {
            set { _applySequence = value; }
        }

        public override bool Dispatch(IMessage message) {
            int sequenceNumber = 1;
            int sequenceSize = _handlers.Count;

            foreach(IMessageHandler handler in _handlers) {
                IMessage messageToSend = (!_applySequence) ? message
                    : MessageBuilder.FromMessage(message)
                            .SetSequenceNumber(sequenceNumber++)
                            .SetSequenceSize(sequenceSize)
                            .Build();

                IExecutor executor = TaskExecutor;
                if(executor != null) {
                    // copy to local variable, because C# does not have real lexical closures. 
                    // see http://blogs.msdn.com/abhinaba/archive/2005/10/18/482180.aspx for an 
                    // explanation
                    IMessageHandler closure = handler;
                    executor.Execute(delegate { SendMessageToHandler(messageToSend, closure); });
                }
                else {
                    SendMessageToHandler(messageToSend, handler);
                }
            }
            return true;
        }
    }
}
