//#region License

///*
// * Copyright 2002-2009 the original author or authors.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *      http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */

//#endregion

//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;
//using Spring.Integration.Channel;
//using Spring.Integration.Core;
//using Spring.Integration.Handler;
//using Spring.Integration.Message;
//using Spring.Objects.Factory.Support;

//namespace Spring.Integration.Tests.Handler {
//    /// <author>Mark Fisher</author>
//    /// <author>Andreas Doehring (.NET)</author>
//    [TestFixture]
//    public class MessageHandlerChainTests {

//        [Test]
//        public void ChainWithOutputChannel() {
//            QueueChannel outputChannel = new QueueChannel();
//            IList<IMessageHandler> handlers = new List<IMessageHandler>();
//            handlers.Add(CreateHandler(1));
//            handlers.Add(CreateHandler(2));
//            handlers.Add(CreateHandler(3));
//            MessageHandlerChain chain = new MessageHandlerChain();
//            chain.ObjectName = "testChain";
//            chain.Handlers = handlers;
//            chain.OutputChannel = outputChannel;
//            chain.HandleMessage(new StringMessage("test"));
//            IMessage reply = outputChannel.Receive(TimeSpan.Zero);
//            Assert.IsNotNull(reply);
//            Assert.That(reply.Payload, Is.EqualTo("test123"));
//        }

//        [Test, Ignore("TODO")] //TODO (expected = IllegalArgumentException.class)
//        public void ChainWithOutputChannelButLastHandlerDoesNotProduceReplies() {
//            QueueChannel outputChannel = new QueueChannel();
//            IList<IMessageHandler> handlers = new List<IMessageHandler>();
//            handlers.Add(CreateHandler(1));
//            handlers.Add(CreateHandler(2));
//            handlers.Add(new InvalidHandler());
//            MessageHandlerChain chain = new MessageHandlerChain();
//            chain.ObjectName = "testChain";
//            chain.Handlers = handlers;
//            chain.OutputChannel = outputChannel;
//            chain.HandleMessage(new StringMessage("test"));
//        }

//        [Test]
//        public void ChainForwardsToReplyChannel() {
//            QueueChannel replyChannel = new QueueChannel();
//            IList<IMessageHandler> handlers = new List<IMessageHandler>();
//            handlers.Add(CreateHandler(1));
//            handlers.Add(CreateHandler(2));
//            handlers.Add(CreateHandler(3));
//            MessageHandlerChain chain = new MessageHandlerChain();
//            chain.ObjectName = "testChain";
//            chain.Handlers = handlers;
//            IMessage message = MessageBuilder.WithPayload("test").SetReplyChannel(replyChannel).Build();
//            chain.HandleMessage(message);
//            IMessage reply = replyChannel.Receive(TimeSpan.Zero);
//            Assert.IsNotNull(reply);
//            Assert.That(reply.Payload, Is.EqualTo("test123"));
//        }

//        [Test]
//        public void chainResolvesReplyChannelName() {
//            QueueChannel replyChannel = new QueueChannel();
//            DefaultListableObjectFactory beanFactory = new DefaultListableObjectFactory();
//            beanFactory.RegisterSingleton("testChannel", replyChannel);
//            IList<IMessageHandler> handlers = new List<IMessageHandler>();
//            handlers.Add(CreateHandler(1));
//            handlers.Add(CreateHandler(2));
//            handlers.Add(CreateHandler(3));
//            MessageHandlerChain chain = new MessageHandlerChain();
//            chain.ObjectName = "testChain";
//            chain.Handlers = handlers;
//            chain.ObjectFactory = beanFactory;
//            IMessage message = MessageBuilder.WithPayload("test").SetReplyChannelName("testChannel").Build();
//            chain.HandleMessage(message);
//            IMessage reply = replyChannel.Receive(TimeSpan.Zero);
//            Assert.IsNotNull(reply);
//            Assert.That(reply.Payload, Is.EqualTo("test123"));
//        }

//        private class InvalidHandler : IMessageHandler {
//            public void HandleMessage(IMessage message) {
//            }
//        }

//        private class TestHandler : AbstractReplyProducingMessageHandler {
//            private readonly int _index;

//            public TestHandler(int index) {
//                _index = index;
//            }
//            protected override void HandleRequestMessage(IMessage requestMessage, ReplyMessageHolder replyMessageHolder) {
//                replyMessageHolder.Set(requestMessage.Payload + _index.ToString());
//            }
//        };

//        private static IMessageHandler CreateHandler(int index) {
//            return new TestHandler(index);
//        }
//    }
//}
