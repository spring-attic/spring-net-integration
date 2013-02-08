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

namespace Spring.Integration.Handler {
    /// <summary>
    /// A simple MessageHandler implementation that passes the request Message
    /// directly to the output channel without modifying it. The main purpose of
    /// this handler is to bridge a PollableChannel to a SubscribableChannel or
    /// vice-versa.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class BridgeHandler : AbstractReplyProducingMessageHandler {

        protected override void HandleRequestMessage(IMessage requestMessage, ReplyMessageHolder replyMessageHolder) {
            replyMessageHolder.Set(requestMessage);
        }
    }
}
