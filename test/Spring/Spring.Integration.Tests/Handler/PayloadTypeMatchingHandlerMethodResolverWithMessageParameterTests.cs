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
using Spring.Integration.Core.Generic;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Message.Generic;

#endregion

namespace Spring.Integration.Tests.Handler
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class PayloadTypeMatchingHandlerMethodResolverWithMessageParameterTests
    {
        private PayloadTypeMatchingHandlerMethodResolver _resolver;


        [SetUp]
        public void InitResolver()
        {
            IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(new TestService());
            _resolver = new PayloadTypeMatchingHandlerMethodResolver(candidates);
        }


        [Test]
        public void StringPayload()
        {
            Type[] types = new[] {typeof (IMessage<string>)};
            MethodInfo expected = typeof (TestService).GetMethod("StringPayload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new StringMessage("foo"));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void ExactMatch()
        {
            Type[] types = new[] {typeof (IMessage<TestFooImpl1>)};
            MethodInfo expected = typeof (TestService).GetMethod("FooImpl1Payload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<ITestFoo>(new TestFooImpl1()));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void InterfaceMatch()
        {
            Type[] types = new[] {typeof (IMessage<ITestFoo>)};
            MethodInfo expected = typeof (TestService).GetMethod("FooInterfacePayload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<ITestFoo>(new TestFooImpl2()));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void SuperclassMatch()
        {
            Type[] types = new[] {typeof (IMessage<TestFooImpl1>)};
            MethodInfo expected = typeof (TestService).GetMethod("FooImpl1Payload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<ITestFoo>(new TestFooImpl1Subclass()));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void InterfaceOfSuperclassMatch()
        {
            Type[] types = new[] {typeof (IMessage<ITestFoo>)};
            MethodInfo expected = typeof (TestService).GetMethod("FooInterfacePayload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<ITestFoo>(new TestFooImpl2Subclass()));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void NumberSuperclassMatch()
        {
            Type[] types = new[] {typeof (IMessage<long>)};
            MethodInfo expected = typeof (TestService).GetMethod("NumberPayload", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<long>(99));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void PayloadAndHeaderMethod()
        {
            Type[] types = new[] {typeof (IMessage<int>), typeof (string)};
            MethodInfo expected = typeof (TestService).GetMethod("IntegerPayloadAndHeader", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<int>(123));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void FallbackToHeaderOnlyMethod()
        {
            Type[] types = new[] {typeof (string)};
            MethodInfo expected = typeof (TestService).GetMethod("HeaderOnlyMethod", types);
            MethodInfo resolved = _resolver.ResolveHandlerMethod(new Message<DateTime>(new DateTime()));
            Assert.That(resolved, Is.EqualTo(expected));
        }

        [Test]
        public void TestStringMessageTypedParameter()
        {
            Object service = new TestServiceWithMessageTypes();
            IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(service);
            PayloadTypeMatchingHandlerMethodResolver methodResovler =
                new PayloadTypeMatchingHandlerMethodResolver(candidates);
            Type[] types = new[] {typeof (StringMessage)};
            MethodInfo expected = typeof (TestServiceWithMessageTypes).GetMethod("StringMessage", types);
            IMessage message = new StringMessage("foo");
            MethodInfo resolved = methodResovler.ResolveHandlerMethod(message);
            Assert.That(resolved, Is.EqualTo(expected));
            Assert.That(resolved.Invoke(service, new object[] {message}), Is.EqualTo("foo"));
        }

        [Test]
        public void TestMessageParameterizedWithString()
        {
            Object service = new TestServiceWithMessageTypes();
            IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(service);
            PayloadTypeMatchingHandlerMethodResolver methodResovler =
                new PayloadTypeMatchingHandlerMethodResolver(candidates);
            Type[] types = new[] {typeof (IMessage<string>)};
            MethodInfo expected = typeof (TestServiceWithMessageTypes).GetMethod("StringParameterizedMessage", types);
            IMessage message = new Message<String>("foo");
            MethodInfo resolved = methodResovler.ResolveHandlerMethod(message);
            Assert.That(resolved, Is.EqualTo(expected));
            Assert.That(resolved.Invoke(service, new object[] {message}), Is.EqualTo("foo"));
        }

        // TODO unbounded wildcard
        //[Test]
        //public void TestUnboundedWildcard() {
        //    Object service = new TestServiceWithMessageTypes();
        //    IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(service);
        //    PayloadTypeMatchingHandlerMethodResolver methodResovler
        //            = new PayloadTypeMatchingHandlerMethodResolver(candidates);
        //    Type[] types = new[] { typeof(IMessage) };
        //    MethodInfo expected = typeof(TestServiceWithMessageTypes).GetMethod("UnboundedWildcardMessage", types);
        //    DateTime date = new DateTime();
        //    IMessage message = new Message<DateTime>(date);
        //    MethodInfo resolved = methodResovler.ResolveHandlerMethod(message);
        //    Assert.That(resolved, Is.EqualTo(expected));
        //    Assert.That(resolved.Invoke(service, new object[] { message }), Is.EqualTo(date));
        //}

        //[Test]
        //public void TestBoundedWildcard() {
        //    Object service = new TestServiceWithMessageTypes();
        //    IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(service);
        //    PayloadTypeMatchingHandlerMethodResolver methodResovler = new PayloadTypeMatchingHandlerMethodResolver(candidates);
        //    Type[] types = new[] { typeof(IMessage) };
        //    MethodInfo expected = typeof(TestServiceWithMessageTypes).GetMethod("boundedWildcardMessage", types);
        //    IMessage message = MessageBuilder.WithPayload(123).Build();
        //    MethodInfo resolved = methodResovler.ResolveHandlerMethod(message);
        //    Assert.That(resolved, Is.EqualTo(expected));
        //    Assert.That(resolved.Invoke(service, new object[] { message }), Is.EqualTo(123));
        //}


        public class TestService
        {
            public void StringPayload(IMessage<string> message)
            {
            }

            public void FooInterfacePayload(IMessage<ITestFoo> message)
            {
            }

            public void FooImpl1Payload(IMessage<TestFooImpl1> message)
            {
            }

            public void NumberPayload(IMessage<long> message)
            {
            }

            public void HeaderOnlyMethod([Header("testHeader")] String s)
            {
            }

            public void IntegerPayloadAndHeader(IMessage<int> message, [Header("testHeader")] string s)
            {
            }
        }


        public class TestServiceWithMessageTypes
        {
            public string StringMessage(StringMessage message)
            {
                return message.TypedPayload;
            }

            public string StringParameterizedMessage(IMessage<string> message)
            {
                return message.TypedPayload;
            }

            public object UnboundedWildcardMessage(IMessage message)
            {
                return message.Payload;
            }

            //public long BoundedWildcardMessage(IMessage<long> message) {
            //    return message.TypedPayload;
            //}
        }


        public interface ITestFoo
        {
        }

        public class TestFooImpl1 : ITestFoo
        {
        }

        public class TestFooImpl2 : ITestFoo
        {
        }

        public class TestFooImpl1Subclass : TestFooImpl1
        {
        }

        public class TestFooImpl2Subclass : TestFooImpl2
        {
        }
    }
}