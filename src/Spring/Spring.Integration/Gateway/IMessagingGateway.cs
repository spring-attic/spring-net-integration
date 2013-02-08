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

namespace Spring.Integration.Gateway {

    /// <summary>
    /// Base interface for gateway adapters. In Spring Integration, a "gateway" is a
    /// component that sends messages to and/or receives messages from message channels
    /// so that application code does not need to be aware of channels or even messages.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    public interface IMessagingGateway {

        void Send(object obj);

        object Receive();

        object SendAndReceive(object obj);

        IMessage SendAndReceiveMessage(object obj);
    }
}
