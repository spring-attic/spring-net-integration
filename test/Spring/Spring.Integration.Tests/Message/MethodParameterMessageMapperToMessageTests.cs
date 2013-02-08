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

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class MethodParameterMessageMapperToMessageTests
    {
        [Test]
        public void ToMessageWithPayload()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayload");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = mapper.ToMessage(new object[] {"test"});
            Assert.That(message.Payload, Is.EqualTo("test"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void ToMessageWithTooManyParameters()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayload");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.ToMessage(new object[] {"test", "oops"});
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void ToMessageWithEmptyParameterArray()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayload");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.ToMessage(new object[] {});
        }

        [Test]
        public void ToMessageWithPayloadAndHeader()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = mapper.ToMessage(new object[] {"test", "bar"});
            Assert.That(message.Payload, Is.EqualTo("test"));
            Assert.That(message.Headers["foo"], Is.EqualTo("bar"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void ToMessageWithPayloadAndRequiredHeaderButNullValue()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.ToMessage(new object[] {"test", null});
        }

        [Test]
        public void ToMessageWithPayloadAndOptionalHeaderWithValueProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndOptionalHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = mapper.ToMessage(new object[] {"test", "bar"});
            Assert.That(message.Payload, Is.EqualTo("test"));
            Assert.That(message.Headers["foo"], Is.EqualTo("bar"));
        }

        [Test]
        public void ToMessageWithPayloadAndOptionalHeaderWithNullValue()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndOptionalHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = mapper.ToMessage(new object[] {"test", null});
            Assert.That(message.Payload, Is.EqualTo("test"));
            Assert.IsFalse(message.Headers.ContainsKey("foo"));
        }

        [Test]
        public void ToMessageWithPayloadAndHeadersMap()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndHeadersMap");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IDictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("abc", 123);
            headers.Add("def", 456);
            IMessage message = mapper.ToMessage(new object[] {"test", headers});
            Assert.That(message.Payload, Is.EqualTo("test"));
            Assert.That(message.Headers["abc"], Is.EqualTo(123));
            Assert.That(message.Headers["def"], Is.EqualTo(456));
        }

        [Test]
        public void ToMessageWithPayloadAndNullHeadersMap()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndHeadersMap");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = mapper.ToMessage(new object[] {"test", null});
            Assert.That(message.Payload, Is.EqualTo("test"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void ToMessageWithPayloadAndHeadersMapWithNonStringKey()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendPayloadAndHeadersMap");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IDictionary<int, string> headers = new Dictionary<int, string>();
            headers.Add(123, "abc");
            mapper.ToMessage(new object[] {"test", headers});
        }

        [Test]
        public void ToMessageWithMessageParameter()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendMessage");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage inputMessage = MessageBuilder.WithPayload("test message").Build();
            IMessage message = mapper.ToMessage(new object[] {inputMessage});
            Assert.That(message.Payload, Is.EqualTo("test message"));
        }

        [Test]
        public void ToMessageWithMessageParameterAndHeader()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendMessageAndHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage inputMessage = MessageBuilder.WithPayload("test message").Build();
            IMessage message = mapper.ToMessage(new object[] {inputMessage, "bar"});
            Assert.That(message.Payload, Is.EqualTo("test message"));
            Assert.That(message.Headers["foo"], Is.EqualTo("bar"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void ToMessageWithMessageParameterAndRequiredHeaderButNullValue()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendMessageAndHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage inputMessage = MessageBuilder.WithPayload("test message").Build();
            mapper.ToMessage(new object[] {inputMessage, null});
        }

        [Test]
        public void ToMessageWithMessageParameterAndOptionalHeaderWithValue()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendMessageAndOptionalHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage inputMessage = MessageBuilder.WithPayload("test message").Build();
            IMessage message = mapper.ToMessage(new object[] {inputMessage, "bar"});
            Assert.That(message.Payload, Is.EqualTo("test message"));
            Assert.That(message.Headers["foo"], Is.EqualTo("bar"));
        }

        [Test]
        public void ToMessageWithMessageParameterAndOptionalHeaderWithNull()
        {
            MethodInfo method = typeof (TestService).GetMethod("SendMessageAndOptionalHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage inputMessage = MessageBuilder.WithPayload("test message").Build();
            IMessage message = mapper.ToMessage(new object[] {inputMessage, null});
            Assert.That(message.Payload, Is.EqualTo("test message"));
            Assert.That(message.Headers.ContainsKey("foo"), Is.False);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void NoArgs()
        {
            MethodInfo method = typeof (TestService).GetMethod("NoArgs");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.ToMessage(new object[] {});
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void OnlyHeaders()
        {
            MethodInfo method = typeof (TestService).GetMethod("OnlyHeaders");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.ToMessage(new object[] {"abc", "def"});
        }


        private interface TestService
        {
            void SendPayload(string payload);

            void SendPayloadAndHeader(string payload, [Header("foo")] string foo);

            void SendPayloadAndOptionalHeader(string payload, [Header(Value = "foo", Required = false)] string foo);

            void SendPayloadAndHeadersMap(string payload, [Headers] IDictionary<string, object> headers);

            void SendMessage(IMessage message);

            void SendMessageAndHeader(IMessage message, [Header("foo")] string foo);

            void SendMessageAndOptionalHeader(IMessage message, [Header(Value = "foo", Required = false)] string foo);

            // invalid methods

            void NoArgs();

            void OnlyHeaders([Header("foo")] string foo, [Header("bar")] string bar);
        }
    }
}