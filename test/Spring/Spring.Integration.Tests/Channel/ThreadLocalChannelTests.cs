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

#region

using System.Collections.Generic;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Channel
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class ThreadLocalChannelTests
    {
        [Test]
        public void testSendAndReceive()
        {
            ThreadLocalChannel channel = new ThreadLocalChannel();
            StringMessage message = new StringMessage("test");
            Assert.IsNull(channel.Receive());
            Assert.IsTrue(channel.Send(message));
            IMessage response = channel.Receive();
            Assert.IsNotNull(response);
            Assert.That(message, Is.EqualTo(response));
            Assert.IsNull(channel.Receive());
        }

        [Test]
        public void testSendAndClear()
        {
            ThreadLocalChannel channel = new ThreadLocalChannel();
            StringMessage message1 = new StringMessage("test1");
            StringMessage message2 = new StringMessage("test2");
            Assert.IsNull(channel.Receive());
            Assert.IsTrue(channel.Send(message1));
            Assert.IsTrue(channel.Send(message2));
            IList<IMessage> clearedMessages = channel.Clear();
            Assert.That(clearedMessages.Count, Is.EqualTo(2));
            Assert.That(clearedMessages[0], Is.EqualTo(message1));
            Assert.That(clearedMessages[1], Is.EqualTo(message2));
            Assert.IsNull(channel.Receive());
        }
    }
}