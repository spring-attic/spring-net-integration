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
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Collections.Generic;
using Apache.NMS;
using Apache.NMS.ActiveMQ.OpenWire;
using NUnit.Framework;
using Spring.Integration.Message;
using IMessage=Spring.Integration.Core.IMessage;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]    
    public class DefaultNmsHeaderMapperTests
    {
        [Test]
        public void JmsReplyToMappedFromHeader()
        {
            Apache.NMS.IDestination replyTo = new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue();
            IMessage message = MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.REPLY_TO, replyTo).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSReplyTo, Is.Not.Null);
            Assert.That(replyTo, Is.SameAs(nmsMessage.NMSReplyTo));
        }

        [Test]
        public void NmsReplyToIgnoredIfIncorrectType()
        {
            IMessage message =
                MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.REPLY_TO, "not-a-destination").Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSReplyTo, Is.Null);               
        }

        [Test]
        public void NmsCorrelationIdMappedFromHeader()
        {
            string nmsCorrelationId = "ABC-123";
            IMessage message =
                MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.CORRELATION_ID, nmsCorrelationId).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSCorrelationID, Is.Not.Null);
            Assert.That(nmsCorrelationId, Is.EqualTo(nmsMessage.NMSCorrelationID));
        }

        [Test]
        public void NmsCorrelationIdIgnoredIfIncorrectType()
        {
            IMessage message = MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.CORRELATION_ID, 123).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSCorrelationID, Is.Null);            
        }

        [Test]
        public void NmsTypeMappedFromHeader()
        {
            string nmsType = "testing";
            IMessage message = MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.TYPE, nmsType).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSType, Is.Not.Null);
            Assert.That(nmsType, Is.EqualTo(nmsMessage.NMSType));
        }

        [Test]
        public void NmsTypeIgnoredIfIncorrectType()
        {
            IMessage message = MessageBuilder.WithPayload("test").SetHeader(NmsHeaders.TYPE, 123).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            Assert.That(nmsMessage.NMSType, Is.Null);   
        }

        [Test]
        public void UserDefinedPropertyMappedFromHeader()
        {
            IMessage message = MessageBuilder.WithPayload("test").SetHeader("foo", 123).Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            mapper.FromHeaders(message.Headers, nmsMessage);
            object value = nmsMessage.Properties["foo"];
            Assert.That(value, Is.Not.Null);
            Assert.That(typeof(int), Is.EqualTo(value.GetType()));
            Assert.That(123, Is.EqualTo(value));
        }

        [Test]
        public void UserDefinedPropertyWithUnsupportedType()
        {            
            Apache.NMS.IDestination replyTo = new Apache.NMS.ActiveMQ.Commands.ActiveMQQueue();
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            nmsMessage.NMSReplyTo = replyTo;
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            IDictionary<string, object> headers = mapper.ToHeaders(nmsMessage);
            object attrib = headers[NmsHeaders.REPLY_TO];
            Assert.That(attrib, Is.Not.Null);
            Assert.That(replyTo, Is.SameAs(attrib));
        }

        [Test]
        public void NmsMessageIdMappedToHeader()
        {
            string messageId = "ID:ABC-123";
            StubTextMessage nmsMessage = new StubTextMessage();
            nmsMessage.NMSMessageId = messageId;
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            IDictionary<string, object> headers = mapper.ToHeaders(nmsMessage);
            object attrib = headers[NmsHeaders.MESSAGE_ID];
            Assert.That(attrib, Is.Not.Null);
            Assert.That(messageId, Is.SameAs(attrib));
        }

        [Test]
        public void NmsCorrelationIdMappedToHeader()
        {            
            string correlationId = "ABC-123";
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            nmsMessage.NMSCorrelationID = correlationId;
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            IDictionary<string, object> headers = mapper.ToHeaders(nmsMessage);
            object attrib = headers[NmsHeaders.CORRELATION_ID];
            Assert.That(attrib, Is.Not.Null);
            Assert.That(correlationId, Is.SameAs(attrib));
        }

        [Test]
        public void NmsTypeMappedToHeader()
        {
            string type = "testing";
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            nmsMessage.NMSType = type;
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            IDictionary<string, object> headers = mapper.ToHeaders(nmsMessage);
            object attrib = headers[NmsHeaders.TYPE];
            Assert.That(attrib, Is.Not.Null);
            Assert.That(type, Is.SameAs(attrib));
        }

        [Test]
        public void UserDefinedPropertyMappedToHeader()
        {            
            Apache.NMS.IMessage nmsMessage = new StubTextMessage();
            nmsMessage.Properties.SetInt("foo", 123);
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            IDictionary<string, object> headers = mapper.ToHeaders(nmsMessage);
            object attrib = headers["foo"];
            Assert.That(attrib, Is.Not.Null);
            Assert.That(typeof(int), Is.EqualTo(attrib.GetType()));
            Assert.That(123, Is.EqualTo(attrib));
        }

        [Test]
        public void PropertyMappingExceptionIsNotFatal()
        {
            IMessage message = MessageBuilder.WithPayload("test")
                .SetHeader("foo", 123)
                .SetHeader("bad", 456)
                .SetHeader("bar", 789)
                .Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            StubTextMessage nmsMessage = new StubTextMessage();
            nmsMessage.Properties = new NMSExceptionThrowingPrimitiveMap();
            mapper.FromHeaders(message.Headers, nmsMessage);
            int foo = nmsMessage.Properties.GetInt("foo");
            Assert.That(foo, Is.EqualTo(123));
            int bar = nmsMessage.Properties.GetInt("bar");
            Assert.That(bar, Is.EqualTo(789));
            Assert.That(nmsMessage.Properties.Contains("bad"), Is.False);
        }

        [Test]
        public void IllegalArgumentExceptionIsNotFatal()
        {
            IMessage message = MessageBuilder.WithPayload("test")
                .SetHeader("foo", 123)
                .SetHeader("bad", 456)
                .SetHeader("bar", 789)
                .Build();
            DefaultNmsHeaderMapper mapper = new DefaultNmsHeaderMapper();
            StubTextMessage nmsMessage = new StubTextMessage();
            nmsMessage.Properties = new BadPrimitiveMap();
            mapper.FromHeaders(message.Headers, nmsMessage);
            int foo = nmsMessage.Properties.GetInt("foo");
            Assert.That(foo, Is.EqualTo(123));
            int bar = nmsMessage.Properties.GetInt("bar");
            Assert.That(bar, Is.EqualTo(789));
            Assert.That(nmsMessage.Properties.Contains("bad"), Is.False);
        }
    }

    public class NMSExceptionThrowingPrimitiveMap : PrimitiveMap
    {
        protected override void SetValue(string name, object value)
        {
            if (name.Equals("bad"))
            {
                throw new NMSException("illegal property");
            }
            base.SetValue(name, value);
        }
    }

    public class BadPrimitiveMap : PrimitiveMap
    {
        protected override void SetValue(string name, object value)
        {
            if (name.Equals("bad"))
            {
                throw new InvalidOperationException("illegal property");
            }
            base.SetValue(name, value);
        }
    }
}