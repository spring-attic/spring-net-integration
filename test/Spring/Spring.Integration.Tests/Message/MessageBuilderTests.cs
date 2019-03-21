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

#endregion

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class MessageBuilderTests
    {
        [Test]
        public void TestSimpleMessageCreation()
        {
            IMessage message = MessageBuilder.WithPayload("foo").Build();

            Assert.That(message.Payload, Is.EqualTo("foo"));
        }

        [Test]
        public void TestHeaderValues()
        {
            IMessage message = MessageBuilder.WithPayload("test")
                .SetHeader("foo", "bar")
                .SetHeader("count", 123)
                .Build();

            Assert.That(message.Headers["foo"], Is.EqualTo("bar"));
            Assert.That(message.Headers["count"], Is.EqualTo(123));
        }

        [Test]
        public void TestCopiedHeaderValues()
        {
            IMessage message1 = MessageBuilder.WithPayload("test1")
                .SetHeader("foo", "1")
                .SetHeader("bar", "2")
                .Build();
            IMessage message2 = MessageBuilder.WithPayload("test2")
                .CopyHeaders(message1.Headers)
                .SetHeader("foo", "42")
                .SetHeaderIfAbsent("bar", "99")
                .Build();

            Assert.That(message1.Payload, Is.EqualTo("test1"));
            Assert.That(message2.Payload, Is.EqualTo("test2"));

            Assert.That(message1.Headers.Get("foo"), Is.EqualTo("1"));
            Assert.That(message2.Headers.Get("foo"), Is.EqualTo("42"));
            Assert.That(message1.Headers.Get("bar"), Is.EqualTo("2"));
            Assert.That(message2.Headers.Get("bar"), Is.EqualTo("2"));
        }

        [Test]
        public void CopyHeadersIfAbsent()
        {
            IMessage message1 = MessageBuilder.WithPayload("test1")
                .SetHeader("foo", "bar").Build();
            IMessage message2 = MessageBuilder.WithPayload("test2")
                .SetHeader("foo", 123)
                .CopyHeadersIfAbsent(message1.Headers)
                .Build();

            Assert.That(message2.Payload, Is.EqualTo("test2"));
            Assert.That(message2.Headers.Get("foo"), Is.EqualTo(123));
        }

        [Test]
        public void CreateFromMessage()
        {
            IMessage message1 = MessageBuilder.WithPayload("test")
                .SetHeader("foo", "bar").Build();
            IMessage message2 = MessageBuilder.FromMessage(message1).Build();

            Assert.That(message2.Payload, Is.EqualTo("test"));
            Assert.That(message2.Headers.Get("foo"), Is.EqualTo("bar"));
        }

        [Test]
        public void TestPriority()
        {
            IMessage importantMessage = MessageBuilder.WithPayload(1)
                .SetPriority(MessagePriority.HIGHEST).Build();

            Assert.That(importantMessage.Headers.Priority, Is.EqualTo(MessagePriority.HIGHEST));
        }

        [Test]
        public void TestNonDestructiveSet()
        {
            IMessage message1 = MessageBuilder.WithPayload(1)
                .SetPriority(MessagePriority.HIGHEST).Build();
            IMessage message2 = MessageBuilder.FromMessage(message1)
                .SetHeaderIfAbsent(MessageHeaders.PRIORITY, MessagePriority.LOW)
                .Build();

            Assert.That(message2.Headers.Priority, Is.EqualTo(MessagePriority.HIGHEST));
        }

        [Test] //, Ignore("check")]
        public void TestExpirationDateSetAsLong()
        {
            DateTime past = DateTime.UtcNow - new TimeSpan(60*1000);

            IMessage expiredMessage = MessageBuilder.WithPayload(1).SetExpirationDate(past).Build();

            Assert.That(expiredMessage.Headers.ExpirationDate, Is.EqualTo(past));
        }

        [Test]
        public void TestRemove()
        {
            IMessage message1 = MessageBuilder.WithPayload(1)
                .SetHeader("foo", "bar").Build();
            IMessage message2 = MessageBuilder.FromMessage(message1)
                .RemoveHeader("foo")
                .Build();

            Assert.That(message2.Headers.ContainsKey("foo"), Is.False);
        }

        [Test]
        public void TestSettingToNullRemoves()
        {
            IMessage message1 = MessageBuilder.WithPayload(1)
                .SetHeader("foo", "bar").Build();
            IMessage message2 = MessageBuilder.FromMessage(message1)
                .SetHeader("foo", null)
                .Build();

            Assert.That(message2.Headers.ContainsKey("foo"), Is.False);
        }
    }
}