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
using System.Collections.Generic;
using NUnit.Framework;
using Spring.Context;
using Spring.Integration.Aggregator;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class ResequencerParserTests
    {
        private IApplicationContext context;


        [SetUp]
        public void setUp()
        {
            context = TestUtils.GetContext(@"Config\resequencerParserTests.xml");
        }


        [Test]
        public void testResequencing()
        {
            IMessageChannel inputChannel = (IMessageChannel) context.GetObject("inputChannel");
            IPollableChannel outputChannel = (IPollableChannel) context.GetObject("outputChannel");
            IList<IMessage> outboundMessages = new List<IMessage>();
            outboundMessages.Add(CreateMessage("123", "id1", 3, 3, outputChannel));
            outboundMessages.Add(CreateMessage("789", "id1", 3, 1, outputChannel));
            outboundMessages.Add(CreateMessage("456", "id1", 3, 2, outputChannel));
            foreach (IMessage message in outboundMessages)
            {
                inputChannel.Send(message);
            }
            IMessage message1 = outputChannel.Receive(TimeSpan.FromMilliseconds(500));
            IMessage message2 = outputChannel.Receive(TimeSpan.FromMilliseconds(500));
            IMessage message3 = outputChannel.Receive(TimeSpan.FromMilliseconds(500));
            Assert.IsNotNull(message1);
            Assert.That(message1.Headers.SequenceNumber, Is.EqualTo(1));
            Assert.IsNotNull(message2);
            Assert.That(message2.Headers.SequenceNumber, Is.EqualTo(2));
            Assert.IsNotNull(message3);
            Assert.That(message3.Headers.SequenceNumber, Is.EqualTo(3));
        }

        [Test]
        public void testDefaultResequencerProperties()
        {
            EventDrivenConsumer endpoint = (EventDrivenConsumer) context.GetObject("defaultResequencer");
            Resequencer resequencer = (Resequencer) TestUtils.GetFieldValue(endpoint, "_handler");
            Assert.IsNull(TestUtils.GetFieldValue(resequencer, "_outputChannel"));
            Assert.IsNull(TestUtils.GetFieldValue(resequencer, "_discardChannel"));
            MessageChannelTemplate channelTemplate =
                (MessageChannelTemplate) TestUtils.GetFieldValue(resequencer, "_channelTemplate");
            Assert.That(TestUtils.GetFieldValue(channelTemplate, "_sendTimeout"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(1000l)),
                        "The ResequencerEndpoint is not set with the appropriate timeout value");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_sendPartialResultOnTimeout"), Is.EqualTo(false),
                        "The ResequencerEndpoint is not configured with the appropriate 'send partial results on timeout' flag");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_reaperInterval"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(1000l)),
                        "The ResequencerEndpoint is not configured with the appropriate reaper interval");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_trackedCorrelationIdCapacity"), Is.EqualTo(1000),
                        "The ResequencerEndpoint is not configured with the appropriate tracked correlationId capacity");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_timeout"), Is.EqualTo(TimeSpan.FromMilliseconds(60000l)),
                        "The ResequencerEndpoint is not configured with the appropriate timeout");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_releasePartialSequences"), Is.EqualTo(true),
                        "The ResequencerEndpoint is not configured with the appropriate 'release partial sequences' flag");
        }

        [Test]
        public void testPropertyAssignment()
        {
            EventDrivenConsumer endpoint = (EventDrivenConsumer) context.GetObject("completelyDefinedResequencer");
            IMessageChannel outputChannel = (IMessageChannel) context.GetObject("outputChannel");
            IMessageChannel discardChannel = (IMessageChannel) context.GetObject("discardChannel");
            Resequencer resequencer = (Resequencer) TestUtils.GetFieldValue(endpoint, "_handler");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_outputChannel"), Is.EqualTo(outputChannel),
                        "The ResequencerEndpoint is not injected with the appropriate output channel");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_discardChannel"), Is.EqualTo(discardChannel),
                        "The ResequencerEndpoint is not injected with the appropriate discard channel");
            MessageChannelTemplate channelTemplate =
                (MessageChannelTemplate) TestUtils.GetFieldValue(resequencer, "_channelTemplate");
            Assert.That(TestUtils.GetFieldValue(channelTemplate, "_sendTimeout"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(86420000l)),
                        "The ResequencerEndpoint is not set with the appropriate timeout value");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_sendPartialResultOnTimeout"), Is.EqualTo(true),
                        "The ResequencerEndpoint is not configured with the appropriate 'send partial results on timeout' flag");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_reaperInterval"),
                        Is.EqualTo(TimeSpan.FromMilliseconds(135l)),
                        "The ResequencerEndpoint is not configured with the appropriate reaper interval");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_trackedCorrelationIdCapacity"), Is.EqualTo(99),
                        "The ResequencerEndpoint is not configured with the appropriate tracked correlationId capacity");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_timeout"), Is.EqualTo(TimeSpan.FromMilliseconds(42l)),
                        "The ResequencerEndpoint is not configured with the appropriate timeout");
            Assert.That(TestUtils.GetFieldValue(resequencer, "_releasePartialSequences"), Is.EqualTo(false),
                        "The ResequencerEndpoint is not configured with the appropriate 'release partial sequences' flag");
        }


        private static IMessage CreateMessage<T>(T payload, object correlationId, int sequenceSize, int sequenceNumber,
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