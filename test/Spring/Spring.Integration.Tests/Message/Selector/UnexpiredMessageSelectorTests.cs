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

using System;
using NUnit.Framework;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Selector;

#endregion

namespace Spring.Integration.Tests.Message.Selector
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class UnexpiredMessageSelectorTests
    {
        [Test]
        public void testExpiredMessageRejected()
        {
            DateTime past = DateTime.Now - new TimeSpan(0, 0, 1, 0);
            IMessage message = MessageBuilder.WithPayload("expired").SetExpirationDate(past).Build();
            UnexpiredMessageSelector selector = new UnexpiredMessageSelector();
            Assert.IsFalse(selector.Accept(message));
        }

        [Test]
        public void testUnexpiredMessageAccepted()
        {
            DateTime future = DateTime.Now + new TimeSpan(0, 0, 1, 0);
            IMessage message = MessageBuilder.WithPayload("unexpired").SetExpirationDate(future).Build();
            UnexpiredMessageSelector selector = new UnexpiredMessageSelector();
            Assert.IsTrue(selector.Accept(message));
        }

        [Test]
        public void testMessageWithNullExpirationDateNeverExpires()
        {
            IMessage message = MessageBuilder.WithPayload("unexpired").Build();
            UnexpiredMessageSelector selector = new UnexpiredMessageSelector();
            Assert.IsTrue(selector.Accept(message));
        }
    }
}