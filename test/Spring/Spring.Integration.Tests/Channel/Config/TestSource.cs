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

using NUnit.Framework;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Channel.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class TestSource : IMessageSource
    {
        private readonly string _text;


        public TestSource(string text)
        {
            _text = text;
        }

        public IMessage Receive()
        {
            return new StringMessage(_text);
        }

        public void Foo()
        {

            IMessage message1 = MessageBuilder.WithPayload("test").SetHeader("foo", "bar").Build();

            IMessage message2 = MessageBuilder.FromMessage(message1).Build();

            Assert.AreEqual("test", message2.Payload);
            Assert.AreEqual("bar", message2.Headers["foo"]);

            IMessage message3 = MessageBuilder.WithPayload("test3")
                    .CopyHeaders(message1.Headers)
                    .Build();

            IMessage message4 = MessageBuilder.WithPayload("test4")
                    .SetHeader("foo", 123)
                    .CopyHeadersIfAbsent(message1.Headers)
                    .Build();

            Assert.AreEqual("bar", message3.Headers["foo"]);
            Assert.AreEqual(123, message4.Headers["foo"]);
        }
    }
}