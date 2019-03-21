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
using System.Reflection;
using NUnit.Framework;
using Spring.Core;
using Spring.Integration.Util;

#endregion

namespace Spring.Integration.Tests.Util
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class DefaultMethodInvokerTests
    {
        [Test]
        public void TestStringArgumentWithVoidReturnAndNoConversionNecessary()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringArgumentWithVoidReturn");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod("test");
            Assert.That(testBean.lastStringArgument, Is.EqualTo("test"));
        }

        [Test]
        public void TestStringArgumentWithVoidReturnAndSuccessfulConversion()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringArgumentWithVoidReturn");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod(123);
            Assert.That(testBean.lastStringArgument, Is.EqualTo("123"));
        }

        [Test]
        public void TestIntegerArgumentWithVoidReturnAndSuccessfulConversion()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("IntegerArgumentWithVoidReturn");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod("123");
            Assert.That(testBean.lastIntegerArgument, Is.EqualTo(123));
        }

        [Test, ExpectedException(typeof (TypeMismatchException))]
        public void TestIntegerArgumentWithVoidReturnAndFailedConversion()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("IntegerArgumentWithVoidReturn");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod("ABC");
        }

        [Test]
        public void TestTwoArgumentsAndNoConversionRequired()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringAndIntegerArgumentMethod");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            object result = invoker.InvokeMethod("ABC", 456);
            Assert.That(result, Is.EqualTo("ABC:456"));
        }

        [Test]
        public void TestTwoArgumentsAndSuccessfulConversion()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringAndIntegerArgumentMethod");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            object result = invoker.InvokeMethod("ABC", "789");
            Assert.That(result, Is.EqualTo("ABC:789"));
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestTwoArgumentMethodWithOnlyOneArgumentProvided()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringAndIntegerArgumentMethod");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod("ABC");
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TestTwoArgumentMethodWithOnlyThreeArgumentsProvided()
        {
            TestBean testBean = new TestBean();
            MethodInfo method = testBean.GetType().GetMethod("StringAndIntegerArgumentMethod");
            DefaultMethodInvoker invoker = new DefaultMethodInvoker(testBean, method);
            invoker.InvokeMethod("ABC", 123, 456);
        }

        private class TestBean
        {
            public string lastStringArgument;
            public int lastIntegerArgument;


            public void StringArgumentWithVoidReturn(string s)
            {
                lastStringArgument = s;
            }

            public void IntegerArgumentWithVoidReturn(int i)
            {
                lastIntegerArgument = i;
            }

            public string StringAndIntegerArgumentMethod(string s, int i)
            {
                return s + ":" + i;
            }
        }
    }
}