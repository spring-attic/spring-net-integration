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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Integration.Core;

#endregion

namespace Spring.Integration.Tests.Core
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class MessageHeadersTests
    {
        [Test]
        public void TestCtorWithNullParameter()
        {
            MessageHeaders headers = new MessageHeaders(null);

            // MessageHeaders do have at least ID and TIMESTAMP
            Assert.That(headers.Count, Is.EqualTo(2));
            Assert.That(headers[MessageHeaders.ID], Is.Not.Null);
            Assert.That(headers[MessageHeaders.TIMESTAMP], Is.Not.Null);
        }

        [Test]
        public void TestCtorAddNewIdAndTimestampIfNotPresent()
        {
            IDictionary<string, object> headers = new Dictionary<string, object>();

            MessageHeaders messageHeaders = new MessageHeaders(headers);
            Assert.That(messageHeaders[MessageHeaders.ID], Is.Not.Null);
            Assert.That(messageHeaders[MessageHeaders.TIMESTAMP], Is.Not.Null);


            headers.Add(MessageHeaders.ID, Guid.NewGuid());
            headers.Add(MessageHeaders.TIMESTAMP, DateTime.Now);

            messageHeaders = new MessageHeaders(headers);

            // ID and Timestamp will always be overwritten
            Assert.That(messageHeaders[MessageHeaders.ID], Is.EqualTo(headers[MessageHeaders.ID]));
            Assert.That(messageHeaders[MessageHeaders.TIMESTAMP], Is.EqualTo(headers[MessageHeaders.TIMESTAMP]));
        }

        [Test]
        public void TestMessageHeadersCopiedFromMap()
        {
            IDictionary<string, object> headerMap = new Dictionary<string, object>();
            headerMap.Add("testAttribute", 123);
            headerMap.Add("testProperty", "foo");
            headerMap.Add(MessageHeaders.SEQUENCE_SIZE, 42);
            headerMap.Add(MessageHeaders.SEQUENCE_NUMBER, 24);

            MessageHeaders messageHeaders = new MessageHeaders(headerMap);

            Assert.That(messageHeaders["testAttribute"], Is.EqualTo(123));
            Assert.That(messageHeaders["testProperty"], Is.EqualTo("foo"));
            Assert.That(messageHeaders.SequenceSize, Is.EqualTo(42));
            Assert.That(messageHeaders.SequenceNumber, Is.EqualTo(24));
        }

        [Test]
        public void TestGetMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);

            Assert.That(messageHeaders.ReplyChannel, Is.Null);
            Assert.That(messageHeaders.SequenceNumber, Is.EqualTo(0));
        }

        [Test]
        public void TestGetIndexer()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);

            Assert.That(messageHeaders[MessageHeaders.ID], Is.Not.Null);
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestAddMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);
            messageHeaders.Add("foo", "bar");
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestRemoveMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);
            messageHeaders.Remove("foo");
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestSetIndexer()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);

            messageHeaders[MessageHeaders.SEQUENCE_SIZE] = 42;
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestAddKeyValuePairMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);
            messageHeaders.Add(new KeyValuePair<string, object>("foo", "bar"));
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestClearMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);
            messageHeaders.Clear();
        }

        [Test]
        [ExpectedException(typeof (NotSupportedException))]
        public void TestRemoveKeyValuePairMethod()
        {
            MessageHeaders messageHeaders = new MessageHeaders(null);
            messageHeaders.Remove(new KeyValuePair<string, object>("foo", "bar"));
        }

        [Test]
        public void TestTimestamp()
        {
            MessageHeaders headers = new MessageHeaders(null);
            Assert.That(headers.Timestamp, Is.Not.Null);
        }

        [Test]
        public void TestNonTypedAccessOfHeaderValue()
        {
            const int value = 123;
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", value);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.That(headers.Get("test"), Is.EqualTo(value));
        }

        [Test]
        public void TestTypedAccessOfHeaderValue()
        {
            const int value = 123;
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", value);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.That(headers.Get<int>("test"), Is.EqualTo(value));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestHeaderValueAccessWithIncorrectType()
        {
            const int value = 123;
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("test", value);
            MessageHeaders headers = new MessageHeaders(map);
            Assert.That(headers.Get<string>("test"), Is.EqualTo(value));
        }

        [Test]
        public void TestNullHeaderValue()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            MessageHeaders headers = new MessageHeaders(map);
            Assert.That(headers.Get("nosuchattribute"), Is.Null);
        }

        [Test]
        public void TestNullHeaderValueWithTypedAccess()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            MessageHeaders headers = new MessageHeaders(map);
            Assert.That(headers.Get<string>("nosuchattribute"), Is.Null);
        }

        [Test]
        public void TestHeaderKeys()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("key1", "val1");
            map.Add("key2", 123);
            MessageHeaders headers = new MessageHeaders(map);
            ICollection<string> keys = headers.Keys;
            Assert.That(keys.Contains("key1"), Is.True);
            Assert.That(keys.Contains("key2"), Is.True);
        }

        [Test]
        public void SerializeWithAllSerializableHeaders()
        {
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Add("name", "joe");
            map.Add("age", 42);
            MessageHeaders input = new MessageHeaders(map);
            MessageHeaders output = (MessageHeaders) SerializeAndDeserialize(input);
            Assert.That(output.Get("name"), Is.EqualTo("joe"));
            Assert.That(output.Get("age"), Is.EqualTo(42));
        }

        // TODO check void SerializeWithNonSerializableHeader()
        //[Test]
        //public void SerializeWithNonSerializableHeader() {
        //    object address = new object();
        //    IDictionary<string, object> map = new Dictionary<string, object>();
        //    map.Add("name", "joe");
        //    map.Add("address", address);
        //    MessageHeaders input = new MessageHeaders(map);
        //    MessageHeaders output = (MessageHeaders)SerializeAndDeserialize(input);
        //    Assert.That(output.Get("name"), Is.EqualTo("joe"));
        //    Assert.That(output.Get("address"), Is.Null);
        //}

        private static object SerializeAndDeserialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream sout = new MemoryStream();
            bf.Serialize(sout, obj);
            Byte[] data = sout.ToArray();
            sout.Close();

            MemoryStream sin = new MemoryStream(data);
            object result = bf.Deserialize(sin);
            sin.Close();

            return result;
        }
    }
}