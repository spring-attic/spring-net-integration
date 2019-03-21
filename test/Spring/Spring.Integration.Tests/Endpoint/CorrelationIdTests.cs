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
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Splitter;

#endregion

namespace Spring.Integration.Tests.Endpoint
{
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class CorrelationIdTests
    {
        [Test]
        public void TestCorrelationIdPassedIfAvailable()
        {
            object correlationId = "123-ABC";
            IMessage message = MessageBuilder.WithPayload("test").SetCorrelationId(correlationId).Build();
            DirectChannel inputChannel = new DirectChannel();
            QueueChannel outputChannel = new QueueChannel(1);
            ServiceActivatingHandler serviceActivator = new ServiceActivatingHandler(new TestBean(), "UpperCase");
            serviceActivator.OutputChannel = outputChannel;
            EventDrivenConsumer endpoint = new EventDrivenConsumer(inputChannel, serviceActivator);
            endpoint.Start();
            Assert.IsTrue(inputChannel.Send(message));
            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.That(reply.Headers.CorrelationId, Is.EqualTo(correlationId));
        }

        [Test]
        public void TestCorrelationIdCopiedFromMessageCorrelationIdIfAvailable()
        {
            IMessage message = MessageBuilder.WithPayload("test").SetCorrelationId("correlationId").Build();
            DirectChannel inputChannel = new DirectChannel();
            QueueChannel outputChannel = new QueueChannel(1);
            ServiceActivatingHandler serviceActivator = new ServiceActivatingHandler(new TestBean(), "UpperCase");
            serviceActivator.OutputChannel = outputChannel;
            EventDrivenConsumer endpoint = new EventDrivenConsumer(inputChannel, serviceActivator);
            endpoint.Start();
            Assert.IsTrue(inputChannel.Send(message));
            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.That(reply.Headers.CorrelationId, Is.EqualTo(message.Headers.CorrelationId));
            Assert.IsTrue(message.Headers.CorrelationId.Equals(reply.Headers.CorrelationId));
        }

        [Test]
        public void TestCorrelationNotPassedFromRequestHeaderIfAlreadySetByHandler()
        {
            object correlationId = "123-ABC";
            IMessage message = MessageBuilder.WithPayload("test").SetCorrelationId(correlationId).Build();
            DirectChannel inputChannel = new DirectChannel();
            QueueChannel outputChannel = new QueueChannel(1);
            ServiceActivatingHandler serviceActivator = new ServiceActivatingHandler(new TestBean(), "CreateMessage");
            serviceActivator.OutputChannel = outputChannel;
            EventDrivenConsumer endpoint = new EventDrivenConsumer(inputChannel, serviceActivator);
            endpoint.Start();
            Assert.IsTrue(inputChannel.Send(message));
            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.That(reply.Headers.CorrelationId, Is.EqualTo("456-XYZ"));
        }

        [Test]
        public void TestCorrelationNotCopiedFromRequestMessgeIdIfAlreadySetByHandler()
        {
            IMessage message = new StringMessage("test");
            DirectChannel inputChannel = new DirectChannel();
            QueueChannel outputChannel = new QueueChannel(1);
            ServiceActivatingHandler serviceActivator = new ServiceActivatingHandler(new TestBean(), "CreateMessage");
            serviceActivator.OutputChannel = outputChannel;
            EventDrivenConsumer endpoint = new EventDrivenConsumer(inputChannel, serviceActivator);
            endpoint.Start();
            Assert.IsTrue(inputChannel.Send(message));
            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.That(reply.Headers.CorrelationId, Is.EqualTo("456-XYZ"));
        }

        [Test]
        public void TestCorrelationIdWithSplitter()
        {
            IMessage message = new StringMessage("test1,test2");
            QueueChannel testChannel = new QueueChannel();
            MethodInvokingSplitter splitter = new MethodInvokingSplitter(new TestBean(),
                                                                         typeof (TestBean).GetMethod("Split"));
            splitter.OutputChannel = testChannel;
            splitter.HandleMessage(message);
            IMessage reply1 = testChannel.Receive(TimeSpan.FromMilliseconds(100));
            IMessage reply2 = testChannel.Receive(TimeSpan.FromMilliseconds(100));
            Assert.That(reply1.Headers.CorrelationId, Is.EqualTo(message.Headers.Id));
            Assert.That(reply2.Headers.CorrelationId, Is.EqualTo(message.Headers.Id));
        }


        private class TestBean
        {
            public string UpperCase(string input)
            {
                return input.ToUpper();
            }

            public string[] Split(string input)
            {
                return input.Split(',');
            }

            public IMessage CreateMessage(string input)
            {
                return MessageBuilder.WithPayload(input).SetCorrelationId("456-XYZ").Build();
            }
        }
    }
}