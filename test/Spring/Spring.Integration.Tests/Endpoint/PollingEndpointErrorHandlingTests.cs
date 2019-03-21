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
using Spring.Context;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Endpoint
{
    /// <author>Jonas Partner</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class PollingEndpointErrorHandlingTests
    {
        [Test]
        public void CheckExceptionPlacedOnErrorChannel()
        {
            IApplicationContext context = TestUtils.GetContext(@"Endpoint\pollingEndpointErrorHandlingTests.xml");
            IPollableChannel errorChannel = (IPollableChannel) context.GetObject("errorChannel");
            IMessage errorMessage = errorChannel.Receive(TimeSpan.FromMilliseconds(5000));
            Assert.IsNotNull(errorMessage, "No error message received");
            Assert.That(errorMessage.GetType(), Is.EqualTo(typeof (ErrorMessage)),
                        "Message received was not an ErrorMessage");
        }
    }
}