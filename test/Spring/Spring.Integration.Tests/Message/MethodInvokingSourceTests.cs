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

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class MethodInvokingSourceTests
    {
        [Test]
        public void TestValidMethod()
        {
            MethodInvokingMessageSource source = new MethodInvokingMessageSource();
            source.Object = new TestObject();
            source.MethodName = "ValidMethod";
            IMessage result = source.Receive();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Payload);
            Assert.That(result.Payload, Is.EqualTo("valid"));
        }

        [Test, ExpectedException(typeof (MessagingException))]
        public void TestNoMatchingMethodName()
        {
            MethodInvokingMessageSource source = new MethodInvokingMessageSource();
            source.Object = new TestObject();
            source.MethodName = "NoMatchingMethod";
            source.Receive();
        }

        [Test, ExpectedException(typeof (MessagingException))]
        public void TestInvalidMethodWithArg()
        {
            MethodInvokingMessageSource source = new MethodInvokingMessageSource();
            source.Object = new TestObject();
            source.MethodName = "InvalidMethodWithArg";
            source.Receive();
        }

        [Test, ExpectedException(typeof (MessagingException))]
        public void TestInvalidMethodWithNoReturnValue()
        {
            MethodInvokingMessageSource source = new MethodInvokingMessageSource();
            source.Object = new TestObject();
            source.MethodName = "InvalidMethodWithNoReturnValue";
            source.Receive();
        }

        private class TestObject
        {
// ReSharper disable UnusedMemberInPrivateClass
            public string ValidMethod()
            {
                return "valid";
            }

            public string InvalidMethodWithArg(string arg)
            {
                return "invalid" + arg;
            }

            public void invalidMethodWithNoReturnValue()
            {
            }

// ReSharper restore UnusedMemberInPrivateClass
        }
    }
}