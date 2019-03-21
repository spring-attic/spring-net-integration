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

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class MethodParameterMessageMapperFromMessageTests
    {
        [Test]
        public void FromMessageWithOptionalHeader()
        {
            MethodInfo method = typeof (TestService).GetMethod("OptionalHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            object[] args = (object[]) mapper.FromMessage(new StringMessage("foo"));
            Assert.That(args.Length, Is.EqualTo(1));
            Assert.IsNull(args[0]);
        }

        [Test, ExpectedException(typeof (MessageHandlingException))]
        public void FromMessageWithRequiredHeaderNotProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("RequiredHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            mapper.FromMessage(new StringMessage("foo"));
        }

        [Test]
        public void FromMessageWithRequiredHeaderProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("RequiredHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = MessageBuilder.WithPayload("foo").SetHeader("num", 123).Build();

            object[] args = (object[]) mapper.FromMessage(message);
            Assert.That(args.Length, Is.EqualTo(1));
            Assert.That(args[0], Is.EqualTo(123));
        }

        [Test, ExpectedException(typeof (MessageHandlingException))]
        public void FromMessageWithOptionalAndRequiredHeaderAndOnlyOptionalHeaderProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("OptionalAndRequiredHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = MessageBuilder.WithPayload("foo").SetHeader("prop", "bar").Build();
            mapper.FromMessage(message);
        }

        [Test]
        public void FromMessageWithOptionalAndRequiredHeaderAndOnlyRequiredHeaderProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("OptionalAndRequiredHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = MessageBuilder.WithPayload("foo").SetHeader("num", 123).Build();
            object[] args = (object[]) mapper.FromMessage(message);
            Assert.That(args.Length, Is.EqualTo(2));
            Assert.IsNull(args[0]);
            Assert.That(args[1], Is.EqualTo(123));
        }

        [Test]
        public void FromMessageWithOptionalAndRequiredHeaderAndBothHeadersProvided()
        {
            MethodInfo method = typeof (TestService).GetMethod("OptionalAndRequiredHeader");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message = MessageBuilder.WithPayload("foo").SetHeader("num", 123).SetHeader("prop", "bar").Build();
            object[] args = (object[]) mapper.FromMessage(message);
            Assert.That(args.Length, Is.EqualTo(2));
            Assert.That(args[0], Is.EqualTo("bar"));
            Assert.That(args[1], Is.EqualTo(123));
        }

        [Test]
        public void FromMessageWithPropertiesMethodAndHeadersAnnotation()
        {
            MethodInfo method = typeof (TestService).GetMethod("PropertiesHeaders");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message =
                MessageBuilder.WithPayload("test").SetHeader("prop1", "foo").SetHeader("prop2", "bar").Build();
            object[] args = (object[]) mapper.FromMessage(message);
            Properties result = (Properties) args[0];
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.GetProperty("prop1"), Is.EqualTo("foo"));
            Assert.That(result.GetProperty("prop2"), Is.EqualTo("bar"));
        }

        [Test]
        public void FromMessageWithPropertiesMethodAndPropertiesPayload()
        {
            MethodInfo method = typeof (TestService).GetMethod("PropertiesPayload");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            Properties payload = new Properties();
            payload.SetProperty("prop1", "foo");
            payload.SetProperty("prop2", "bar");
            IMessage message =
                MessageBuilder.WithPayload(payload).SetHeader("prop1", "not").SetHeader("prop2", "these").Build();
            object[] args = (object[]) mapper.FromMessage(message);
            Properties result = (Properties) args[0];
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.GetProperty("prop1"), Is.EqualTo("foo"));
            Assert.That(result.GetProperty("prop2"), Is.EqualTo("bar"));
        }

        [Test]
        public void FromMessageWithMapMethodAndHeadersAnnotation()
        {
            MethodInfo method = typeof (TestService).GetMethod("MapHeaders");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IMessage message =
                MessageBuilder.WithPayload("test").SetHeader("attrib1", 123).SetHeader("attrib2", 456).Build();
            object[] args = (object[]) mapper.FromMessage(message);
            IDictionary<string, object> result = (IDictionary<string, object>) args[0];
            Assert.That(result["attrib1"], Is.EqualTo(123));
            Assert.That(result["attrib2"], Is.EqualTo(456));
        }

        [Test]
        public void FromMessageWithMapMethodAndMapPayload()
        {
            MethodInfo method = typeof (TestService).GetMethod("MapPayload");
            MethodParameterMessageMapper mapper = new MethodParameterMessageMapper(method);
            IDictionary<string, int> payload = new Dictionary<string, int>();
            payload.Add("attrib1", 123);
            payload.Add("attrib2", 456);
            IMessage message =
                MessageBuilder.WithPayload(payload).SetHeader("attrib1", 123).SetHeader("attrib2", 456).Build();
            object[] args = (object[]) mapper.FromMessage(message);
            IDictionary<string, int> result = (IDictionary<string, int>) args[0];
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result["attrib1"], Is.EqualTo(123));
            Assert.That(result["attrib2"], Is.EqualTo(456));
        }


        private class TestService
        {
            public string MessageOnly(IMessage message)
            {
                return (string) message.Payload;
            }

            public string MessageAndHeader(IMessage message, [Header("number")] int num)
            {
                return (string) message.Payload + "-" + num;
            }

            public string twoHeaders([Header] string prop, [Header("number")] int num)
            {
                return prop + "-" + num;
            }

            public int OptionalHeader([Header(Required = false)] int num)
            {
                return num;
            }

            public int RequiredHeader([Header(Value = "num", Required = true)] int num)
            {
                return num;
            }

            public string OptionalAndRequiredHeader([Header(Required = false)] string prop,
                                                    [Header(Value = "num", Required = true)] int num)
            {
                return prop + num;
            }

            public Properties PropertiesPayload(Properties properties)
            {
                return properties;
            }

            public Properties PropertiesHeaders([Headers] Properties properties)
            {
                return properties;
            }

            //@SuppressWarnings("unchecked")
            public System.Collections.IDictionary MapPayload(System.Collections.IDictionary map)
            {
                return map;
            }

            //@SuppressWarnings("unchecked")
            public System.Collections.IDictionary MapHeaders([Headers] IDictionary map)
            {
                return map;
            }

            public int IntegerMethod(int i)
            {
                return i;
            }
        }
    }
}