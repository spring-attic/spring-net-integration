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
using Spring.Integration.Handler;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Handler
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class StaticHandlerMethodResolverTests
    {
        [Test, ExpectedException(typeof (ArgumentException))]
        public void MethodDeclaredOnObjectIsNotValid()
        {
            MethodInfo method = typeof (object).GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public);
            new StaticHandlerMethodResolver(method);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void NoArgMethodIsNotValid()
        {
            MethodInfo method = typeof (TestBean).GetMethod("NoArgMethod");
            new StaticHandlerMethodResolver(method);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void PrivateMethodIsNotValid()
        {
            MethodInfo method = typeof (TestBean).GetMethod("PrivateMethod",
                                                            BindingFlags.Instance | BindingFlags.NonPublic);
            new StaticHandlerMethodResolver(method);
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void NullMethodIsNotValid()
        {
            new StaticHandlerMethodResolver(null);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void AmbiguousMethodIsNotValid()
        {
            MethodInfo method = typeof (TestBean).GetMethod("AmbiguousMethod");
            new StaticHandlerMethodResolver(method);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void AmbiguousMethodIsNotValid2()
        {
            MethodInfo method = typeof (TestBean).GetMethod("AmbiguousMethod2");
            new StaticHandlerMethodResolver(method);
        }

        [Test]
        public void validPayloadMethod()
        {
            MethodInfo method = typeof (TestBean).GetMethod("PayloadMethod");
            IHandlerMethodResolver resolver = new StaticHandlerMethodResolver(method);
            MethodInfo resolved = resolver.ResolveHandlerMethod(new StringMessage("foo"));
            Assert.That(resolved, Is.EqualTo(method));
        }

        [Test]
        public void validHeaderMethod()
        {
            MethodInfo method = typeof (TestBean).GetMethod("HeaderMethod");
            IHandlerMethodResolver resolver = new StaticHandlerMethodResolver(method);
            MethodInfo resolved = resolver.ResolveHandlerMethod(new StringMessage("foo"));
            Assert.That(resolved, Is.EqualTo(method));
        }

        [Test]
        public void validHeaderMapMethod()
        {
            MethodInfo method = typeof (TestBean).GetMethod("HeaderMapMethod");
            IHandlerMethodResolver resolver = new StaticHandlerMethodResolver(method);
            MethodInfo resolved = resolver.ResolveHandlerMethod(new StringMessage("foo"));
            Assert.That(resolved, Is.EqualTo(method));
        }

        [Test]
        public void validPayloadAndHeaderMethod()
        {
            MethodInfo method = typeof (TestBean).GetMethod("PayloadAndHeaderMethod");
            IHandlerMethodResolver resolver = new StaticHandlerMethodResolver(method);
            MethodInfo resolved = resolver.ResolveHandlerMethod(new StringMessage("foo"));
            Assert.That(resolved, Is.EqualTo(method));
        }


        public class TestBean
        {
            public void NoArgMethod()
            {
            }

            private void PrivateMethod(string s)
            {
            }

            public void AmbiguousMethod(string s1, string s2)
            {
            }

            public void AmbiguousMethod2(string s1, [Header] string s2, string s3)
            {
            }

            public void PayloadMethod(string s)
            {
            }

            public void HeaderMethod([Header("test")] string s)
            {
            }

            public void HeaderMapMethod([Headers] IDictionary<string, object> headerMap)
            {
            }

            public void PayloadAndHeaderMethod(string s1, [Header("test")] string s2)
            {
            }
        }
    }
}