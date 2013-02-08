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
using Spring.Context;
using Spring.Integration.Aggregator;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;
using Spring.Integration.Util;
using Spring.Objects.Factory;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Marius Bogoevici</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    [TestFixture]
    public class AggregatorParserTests
    {
        private IApplicationContext _ctx;


        [TestFixtureSetUp]
        public void SetUp()
        {
            _ctx = TestUtils.GetContext(@"Config\AggregatorParserTests.xml");
        }

        [Test]
        public void TestAggregation()
        {
            IMessageChannel input = (IMessageChannel) _ctx.GetObject("aggregatorWithReferenceInput");
            TestAggregatorObject aggregatorObject = (TestAggregatorObject) _ctx.GetObject("aggregatorObject");
            IList<IMessage> outboundMessages = new List<IMessage>();
            outboundMessages.Add(CreateMessage("123", "id1", 3, 1, null));
            outboundMessages.Add(CreateMessage("789", "id1", 3, 3, null));
            outboundMessages.Add(CreateMessage("456", "id1", 3, 2, null));
            foreach (IMessage message in outboundMessages)
            {
                input.Send(message);
            }
            Assert.That(aggregatorObject.AggregatedMessages.Count, Is.EqualTo(1),
                        "One and only one message must have been aggregated");

            IMessage aggregatedMessage = aggregatorObject.AggregatedMessages["id1"];
            Assert.That(aggregatedMessage.Payload, Is.EqualTo("123456789"),
                        "The aggreggated message payload is not correct", "123456789");
        }

        [Test]
        public void TestPropertyAssignment()
        {
            EventDrivenConsumer endpoint = (EventDrivenConsumer) _ctx.GetObject("completelyDefinedAggregator");
            ICompletionStrategy completionStrategy = (ICompletionStrategy) _ctx.GetObject("completionStrategy");
            IMessageChannel outputChannel = (IMessageChannel) _ctx.GetObject("outputChannel");
            IMessageChannel discardChannel = (IMessageChannel) _ctx.GetObject("discardChannel");

            object consumer =
                endpoint.GetType().GetField("_handler", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(
                    endpoint);
            Assert.That(typeof (MethodInvokingAggregator), Is.EqualTo(consumer.GetType()));

            //DirectFieldAccessor accessor = new DirectFieldAccessor(consumer);
            MethodInfo expectedMethod = typeof (TestAggregatorObject).GetMethod("CreateSingleMessageFromGroup");
            Assert.That(TestUtils.GetFieldValue(TestUtils.GetFieldValue(consumer, "_methodInvoker"), "_method"),
                        Is.EqualTo(expectedMethod),
                        "The MethodInvokingAggregator is not injected with the appropriate aggregation method");
            Assert.That(TestUtils.GetFieldValue(consumer, "_completionStrategy"), Is.EqualTo(completionStrategy),
                        "The AggregatorEndpoint is not injected with the appropriate CompletionStrategy instance");
            Assert.That(TestUtils.GetFieldValue(consumer, "_outputChannel"), Is.EqualTo(outputChannel),
                        "The AggregatorEndpoint is not injected with the appropriate output channel");
            Assert.That(TestUtils.GetFieldValue(consumer, "_discardChannel"), Is.EqualTo(discardChannel),
                        "The AggregatorEndpoint is not injected with the appropriate discard channel");
            Assert.That(TestUtils.GetFieldValue(TestUtils.GetFieldValue(consumer, "_channelTemplate"), "_sendTimeout"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(86420000l)),
                        "The AggregatorEndpoint is not set with the appropriate timeout value");
            Assert.That(TestUtils.GetFieldValue(consumer, "_sendPartialResultOnTimeout"), Is.True,
                        "The AggregatorEndpoint is not configured with the appropriate 'send partial results on timeout' flag");
            Assert.That(TestUtils.GetFieldValue(consumer, "_reaperInterval"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(135l)),
                        "The AggregatorEndpoint is not configured with the appropriate reaper interval");
            Assert.That(TestUtils.GetFieldValue(consumer, "_trackedCorrelationIdCapacity"), Is.EqualTo(99),
                        "The AggregatorEndpoint is not configured with the appropriate tracked correlationId capacity");
            Assert.That(TestUtils.GetFieldValue(consumer, "_timeout"), Is.EqualTo(TimeSpan.FromMilliseconds(42l)),
                        "The AggregatorEndpoint is not configured with the appropriate timeout");
        }

        [Test]
        public void TestSimpleObjectAggregator()
        {
            IList<IMessage> outboundMessages = new List<IMessage>();
            IMessageChannel input = (IMessageChannel) _ctx.GetObject("aggregatorWithReferenceAndMethodInput");
            outboundMessages.Add(CreateMessage(1l, "id1", 3, 1, null));
            outboundMessages.Add(CreateMessage(2l, "id1", 3, 3, null));
            outboundMessages.Add(CreateMessage(3l, "id1", 3, 2, null));
            foreach (IMessage message in outboundMessages)
            {
                input.Send(message);
            }
            IPollableChannel outputChannel = (IPollableChannel) _ctx.GetObject("outputChannel");
            IMessage response = outputChannel.Receive();
            Assert.That(response.Payload, Is.EqualTo(6l));
        }

        [Test, ExpectedException(typeof (ObjectCreationException))]
        public void TestMissingMethodOnAggregator()
        {
            _ctx = TestUtils.GetContext(@"Config\invalidMethodNameAggregator.xml");
        }

        [Test, ExpectedException(typeof (ObjectCreationException))]
        public void TestDuplicateCompletionStrategyDefinition()
        {
            _ctx = TestUtils.GetContext(@"Config\completionStrategyMethodWithMissingReference.xml");
        }

        [Test]
        public void TestAggregatorWithPojoCompletionStrategy()
        {
            IMessageChannel input = (IMessageChannel) _ctx.GetObject("aggregatorWithPojoCompletionStrategyInput");
            EventDrivenConsumer endpoint = (EventDrivenConsumer) _ctx.GetObject("aggregatorWithPojoCompletionStrategy");

            ICompletionStrategy completionStrategy =
                (ICompletionStrategy)
                TestUtils.GetFieldValue(TestUtils.GetFieldValue(endpoint, "_handler"), "_completionStrategy");
            Assert.IsTrue(completionStrategy is CompletionStrategyAdapter);

            //DirectFieldAccessor completionStrategyAccessor = new DirectFieldAccessor(completionStrategy);
            IMethodInvoker invoker = (IMethodInvoker) TestUtils.GetFieldValue(completionStrategy, "_invoker");
            Assert.IsTrue(TestUtils.GetFieldValue(invoker, "_obj") is MaxValueCompletionStrategy);
            Assert.IsTrue(
                ((MethodInfo) TestUtils.GetFieldValue(completionStrategy, "_method")).Name.Equals("CheckCompleteness"));
            input.Send(CreateMessage(1l, "id1", 0, 0, null));
            input.Send(CreateMessage(2l, "id1", 0, 0, null));
            input.Send(CreateMessage(3l, "id1", 0, 0, null));
            IPollableChannel outputChannel = (IPollableChannel) _ctx.GetObject("outputChannel");
            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.IsNull(reply);
            input.Send(CreateMessage(5l, "id1", 0, 0, null));
            reply = outputChannel.Receive(TimeSpan.Zero);
            Assert.IsNotNull(reply);
            Assert.That(reply.Payload, Is.EqualTo(11l));
        }

        [Test, ExpectedException(typeof (ObjectCreationException))]
        public void TestAggregatorWithInvalidCompletionStrategyMethod()
        {
            _ctx = TestUtils.GetContext(@"config\InvalidCompletionStrategyMethod.xml");
        }


        private static IMessage CreateMessage<T>(T payload, Object correlationId, int sequenceSize, int sequenceNumber,
                                                 IMessageChannel outputChannel)
        {
            return MessageBuilder.WithPayload(payload)
                .SetCorrelationId(correlationId)
                .SetSequenceSize(sequenceSize)
                .SetSequenceNumber(sequenceNumber)
                .SetReplyChannel(outputChannel)
                .Build();
        }
    }
}