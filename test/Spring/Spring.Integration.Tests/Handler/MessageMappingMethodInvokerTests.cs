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
using System.Reflection;
using NUnit.Framework;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Message.Generic;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Handler
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class MessageMappingMethodInvokerTests
    {
        [Test]
        public void PayloadAsMethodParameterAndObjectAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptPayloadAndReturnObject");
            object result = invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result, Is.EqualTo("testing-1"));
        }

        [Test]
        public void PayloadAsMethodParameterAndMessageAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptPayloadAndReturnMessage");
            IMessage result = (IMessage) invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result.Payload, Is.EqualTo("testing-2"));
        }

        [Test]
        public void MessageAsMethodParameterAndObjectAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptMessageAndReturnObject");
            object result = invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result, Is.EqualTo("testing-3"));
        }

        [Test]
        public void MessageAsMethodParameterAndMessageAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptMessageAndReturnMessage");
            IMessage result = (IMessage) invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result.Payload, Is.EqualTo("testing-4"));
        }

        [Test]
        public void MessageSubclassAsMethodParameterAndMessageAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptMessageSubclassAndReturnMessage");
            IMessage result = (IMessage) invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result.Payload, Is.EqualTo("testing-5"));
        }

        [Test]
        public void MessageSubclassAsMethodParameterAndMessageSubclassAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptMessageSubclassAndReturnMessageSubclass");
            IMessage result = (IMessage) invoker.InvokeMethod(new StringMessage("testing"));
            Assert.That(result.Payload, Is.EqualTo("testing-6"));
        }

        [Test]
        public void PayloadAndHeaderAnnotationMethodParametersAndObjectAsReturnValue()
        {
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(new TestBean(),
                                                                                  "AcceptPayloadAndHeaderAndReturnObject");
            IMessage request = MessageBuilder.WithPayload("testing").SetHeader("number", 123).Build();
            object result = invoker.InvokeMethod(request);
            Assert.That(result, Is.EqualTo("testing-123"));
        }

        [Test]
        public void MessageOnlyWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("MessageOnly");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            object result = invoker.InvokeMethod(new StringMessage("foo"));
            Assert.That(result, Is.EqualTo("foo"));
        }

        [Test]
        public void PayloadWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("IntegerMethod");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            object result = invoker.InvokeMethod(new Message<int>(123));
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void ConvertedPayloadWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("IntegerMethod");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            object result = invoker.InvokeMethod(new StringMessage("456"));
            Assert.That(result, Is.EqualTo(456));
        }

        [Test, ExpectedException(typeof (MessageHandlingException))]
        public void ConversionFailureWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("IntegerMethod");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            object result = invoker.InvokeMethod(new StringMessage("foo"));
            Assert.That(result, Is.EqualTo(123));
        }

        [Test]
        public void MessageAndHeaderWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("MessageAndHeader");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            IMessage message = MessageBuilder.WithPayload("foo").SetHeader("number", 42).Build();
            object result = invoker.InvokeMethod(message);
            Assert.That(result, Is.EqualTo("foo-42"));
        }

        [Test]
        public void MultipleHeadersWithAnnotatedMethod()
        {
            AnnotatedTestService service = new AnnotatedTestService();
            MethodInfo method = service.GetType().GetMethod("TwoHeaders");
            MessageMappingMethodInvoker invoker = new MessageMappingMethodInvoker(service, method);
            IMessage message =
                MessageBuilder.WithPayload("foo").SetHeader("prop", "bar").SetHeader("number", 42).Build();
            object result = invoker.InvokeMethod(message);
            Assert.That(result, Is.EqualTo("bar-42"));
        }


        private class TestBean
        {
            public string AcceptPayloadAndReturnObject(string s)
            {
                return s + "-1";
            }

            public IMessage AcceptPayloadAndReturnMessage(string s)
            {
                return new StringMessage(s + "-2");
            }

            public string AcceptMessageAndReturnObject(IMessage m)
            {
                return m.Payload + "-3";
            }

            public IMessage AcceptMessageAndReturnMessage(IMessage m)
            {
                return new StringMessage(m.Payload + "-4");
            }

            public IMessage AcceptMessageSubclassAndReturnMessage(StringMessage m)
            {
                return new StringMessage(m.Payload + "-5");
            }

            public StringMessage AcceptMessageSubclassAndReturnMessageSubclass(StringMessage m)
            {
                return new StringMessage(m.Payload + "-6");
            }

            public string AcceptPayloadAndHeaderAndReturnObject(string s, [Header("number")] int n)
            {
                return s + "-" + n;
            }
        }

        private class AnnotatedTestService
        {
            public string MessageOnly(IMessage message)
            {
                return (string) message.Payload;
            }

            public string MessageAndHeader(IMessage message, [Header("number")] int num)
            {
                return (string) message.Payload + "-" + num;
            }

            public string TwoHeaders([Header] string prop, [Header("number")] int num)
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

            public Properties PropertiesMethod(Properties properties)
            {
                return properties;
            }

            public IDictionary MapMethod(IDictionary map)
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