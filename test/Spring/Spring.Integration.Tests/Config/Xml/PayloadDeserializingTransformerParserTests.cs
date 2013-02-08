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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message.Generic;
using Spring.Integration.Tests.Util;
using Spring.Integration.Transformer;

#endregion

namespace Spring.Integration.Tests.Config.Xml
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    //@ContextConfiguration
        //@RunWith(SpringJUnit4ClassRunner.class)
    public class PayloadDeserializingTransformerParserTests
    {
        //@Autowired
        //@Qualifier("directInput")
        //private MessageChannel directInput;

        //@Autowired
        //@Qualifier("queueInput")
        //private MessageChannel queueInput;

        //@Autowired
        //@Qualifier("output")
        //private PollableChannel output;


        [Test]
        public void DirectChannelWithSerializedStringMessage()
        {
            IApplicationContext context =
                TestUtils.GetContext(@"Config\Xml\PayloadDeserializingTransformerParserTests-context.xml");
            IMessageChannel directInput = (IMessageChannel) context.GetObject("directInput");
            byte[] bytes = Serialize("foo");
            directInput.Send(new Message<byte[]>(bytes));
            IPollableChannel output = (IPollableChannel) context.GetObject("output");
            IMessage result = output.Receive(TimeSpan.Zero);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Payload is string);
            Assert.That(result.Payload, Is.EqualTo("foo"));
        }

        [Test]
        public void QueueChannelWithSerializedStringMessage()
        {
            IApplicationContext context =
                TestUtils.GetContext(@"Config\Xml\PayloadDeserializingTransformerParserTests-context.xml");
            IMessageChannel queueInput = (IMessageChannel) context.GetObject("queueInput");
            byte[] bytes = Serialize("foo");
            queueInput.Send(new Message<byte[]>(bytes));
            IPollableChannel output = (IPollableChannel) context.GetObject("output");
            IMessage result = output.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Payload is string);
            Assert.That(result.Payload, Is.EqualTo("foo"));
        }

        [Test]
        public void DirectChannelWithSerializedObjectMessage()
        {
            IApplicationContext context =
                TestUtils.GetContext(@"Config\Xml\PayloadDeserializingTransformerParserTests-context.xml");
            IMessageChannel directInput = (IMessageChannel) context.GetObject("directInput");
            byte[] bytes = Serialize(new TestObject());
            directInput.Send(new Message<byte[]>(bytes));
            IPollableChannel output = (IPollableChannel) context.GetObject("output");
            IMessage result = output.Receive(TimeSpan.Zero);
            Assert.IsNotNull(result);
            Assert.That(result.Payload.GetType(), Is.EqualTo(typeof (TestObject)));
            Assert.That(((TestObject) result.Payload).name, Is.EqualTo("test"));
        }

        [Test]
        public void QueueChannelWithSerializedObjectMessage()
        {
            IApplicationContext context =
                TestUtils.GetContext(@"Config\Xml\PayloadDeserializingTransformerParserTests-context.xml");
            IMessageChannel queueInput = (IMessageChannel) context.GetObject("queueInput");
            byte[] bytes = Serialize(new TestObject());
            queueInput.Send(new Message<byte[]>(bytes));
            IPollableChannel output = (IPollableChannel) context.GetObject("output");
            IMessage result = output.Receive(TimeSpan.FromMilliseconds(3000));
            Assert.IsNotNull(result);
            Assert.That(result.Payload.GetType(), Is.EqualTo(typeof (TestObject)));
            Assert.That(((TestObject) result.Payload).name, Is.EqualTo("test"));
        }

        [Test, ExpectedException(typeof (MessageTransformationException))]
        public void InvalidPayload()
        {
            IApplicationContext context =
                TestUtils.GetContext(@"Config\Xml\PayloadDeserializingTransformerParserTests-context.xml");
            IMessageChannel directInput = (IMessageChannel) context.GetObject("directInput");
            directInput.Send(new Message<NonSerializableTestObject>(new NonSerializableTestObject()));
        }


        private static byte[] Serialize(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(ms, obj);
                ms.Flush();
                ms.Close();
                return ms.ToArray();
            }
        }

        [Serializable]
        private class TestObject
        {
            public string name = "test";
        }

        private class NonSerializableTestObject
        {
            public string name = "test";
        }
    }
}