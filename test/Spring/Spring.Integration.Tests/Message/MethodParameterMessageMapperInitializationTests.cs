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
using System.Reflection;
using NUnit.Framework;
using Spring.Integration.Core;
using Spring.Integration.Message;

#endregion

namespace Spring.Integration.Tests.Message
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class MethodParameterMessageMapperInitializationTests
    {
        [Test, ExpectedException(typeof (ArgumentException))]
        public void MessageAndPayload()
        {
            MethodInfo method = typeof (TestService).GetMethod("MessageAndPayload");
            new MethodParameterMessageMapper(method);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TwoMessages()
        {
            MethodInfo method = typeof (TestService).GetMethod("TwoMessages");
            new MethodParameterMessageMapper(method);
        }

        [Test, ExpectedException(typeof (ArgumentException))]
        public void TwoPayloads()
        {
            MethodInfo method = typeof (TestService).GetMethod("TwoPayloads");
            new MethodParameterMessageMapper(method);
        }


        private interface TestService
        {
            void MessageAndPayload(IMessage message, string foo);

            void TwoMessages(IMessage message1, IMessage message2);

            void TwoPayloads(string foo, string bar);
        }
    }
}