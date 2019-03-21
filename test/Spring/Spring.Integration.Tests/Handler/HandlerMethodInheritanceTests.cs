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

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Handler
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class HandlerMethodInheritanceTests
    {
        [Test] // INT-506
        public void overriddenMethodExcludedFromCandidateList()
        {
            IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(new TestSubclass());
            Assert.That(candidates.Count, Is.EqualTo(1));
            MethodInfo expected = ReflectionUtils.GetMethod(typeof (TestSubclass), "Test", new[] {typeof (string)});
            Assert.That(candidates[0], Is.EqualTo(expected));
        }

        [Test] // INT-506
        public void overridingMethodResolves()
        {
            IList<MethodInfo> candidates = HandlerMethodUtils.GetCandidateHandlerMethods(new TestSubclass());
            PayloadTypeMatchingHandlerMethodResolver resolver = new PayloadTypeMatchingHandlerMethodResolver(candidates);
            MethodInfo resolved = resolver.ResolveHandlerMethod(new StringMessage("test"));
            MethodInfo expected = ReflectionUtils.GetMethod(typeof (TestSubclass), "Test", new[] {typeof (string)});
            Assert.That(resolved, Is.EqualTo(expected));
        }


        public class TestSuperclass
        {
            public virtual void Test(string s)
            {
            }
        }


        public class TestSubclass : TestSuperclass
        {
            public override void Test(string s)
            {
            }
        }
    }
}