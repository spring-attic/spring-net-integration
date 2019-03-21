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
using Spring.Integration.Message.Generic;
using Spring.Integration.Selector;

#endregion

namespace Spring.Integration.Tests.Message.Selector
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class PayloadTypeSelectorTests
    {
        [Test]
        public void TestAcceptedTypeIsSelected()
        {
            PayloadTypeSelector selector = new PayloadTypeSelector(typeof (string));
            Assert.IsTrue(selector.Accept(new StringMessage("test")));
        }

        [Test]
        public void TestNonAcceptedTypeIsNotSelected()
        {
            PayloadTypeSelector selector = new PayloadTypeSelector(typeof (int));
            Assert.IsFalse(selector.Accept(new StringMessage("test")));
        }

        [Test]
        public void TestMultipleAcceptedTypes()
        {
            PayloadTypeSelector selector = new PayloadTypeSelector(typeof (string), typeof (int));
            Assert.IsTrue(selector.Accept(new StringMessage("test1")));
            Assert.IsTrue(selector.Accept(new Message<int>(2)));
            Assert.IsFalse(selector.Accept(new ErrorMessage(new Exception())));
        }

        [Test]
        public void TestSubclassOfAcceptedTypeIsSelected()
        {
            PayloadTypeSelector selector = new PayloadTypeSelector(typeof (Exception));
            Assert.IsTrue(selector.Accept(new ErrorMessage(new MessagingException("test"))));
        }

        [Test]
        public void TestSuperclassOfAcceptedTypeIsNotSelected()
        {
            PayloadTypeSelector selector = new PayloadTypeSelector(typeof (MessageHandlingException));
            Assert.IsFalse(selector.Accept(new ErrorMessage(new Exception("test"))));
        }
    }
}