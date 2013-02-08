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

using System.Collections.Generic;
using NUnit.Framework;
using Spring.Integration.Core;
using Spring.Integration.Message.Generic;

#endregion

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void TestMessageHeadersCopiedFromMap()
        {
            IDictionary<string, object> headerMap = new Dictionary<string, object>();
            headerMap.Add("testAttribute", 123);
            headerMap.Add("testProperty", "foo");
            headerMap.Add(MessageHeaders.SEQUENCE_SIZE, 42);
            headerMap.Add(MessageHeaders.SEQUENCE_NUMBER, 24);

            Message<string> message = new Message<string>("test", headerMap);

            Assert.That(message.Headers["testAttribute"], Is.EqualTo(123));
            Assert.That(message.Headers["testProperty"], Is.EqualTo("foo"));
            Assert.That(message.Headers.SequenceSize, Is.EqualTo(42));
            Assert.That(message.Headers.SequenceNumber, Is.EqualTo(24));
        }
    }
}