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
using Rhino.Mocks;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Gateway;
using Spring.Integration.Message;
using Spring.Integration.Tests.Util;

#endregion

// TODO investigate rhino mocks

namespace Spring.Integration.Tests.Gateway
{
    /// <author>Iwein Fuld</author>
    /// <author>Andreas D�hring (.NET)</author>
    [TestFixture]
    public class SimpleMessagingGatewayTests
    {
        private MockRepository _mocks;

        private SimpleMessagingGateway _simpleMessagingGateway;

        private IMessageChannel _requestChannel;

        private IPollableChannel _replyChannel;

        private IMessage _messageMock;


        [SetUp]
        public void InitializeSample()
        {
            _mocks = new MockRepository();
            _requestChannel = _mocks.StrictMock<IMessageChannel>();

            _replyChannel = _mocks.StrictMock<IPollableChannel>();
            _messageMock = _mocks.StrictMock<IMessage>();

            _simpleMessagingGateway = new SimpleMessagingGateway();
            _simpleMessagingGateway.RequestChannel = _requestChannel;
            _simpleMessagingGateway.ReplyChannel = _replyChannel;
            _simpleMessagingGateway.ObjectFactory = TestUtils.CreateTestApplicationContext();
            //reset(_allmocks);
        }


        /* send tests */

        [Test]
        public void SendMessage()
        {
            Expect.Call(_requestChannel.Send(_messageMock)).Return(true);
            _mocks.ReplayAll();
            _simpleMessagingGateway.Send(_messageMock);
            _mocks.VerifyAll();
        }

        [Test, ExpectedException(typeof (MessageDeliveryException))]
        public void SendMessageFailure()
        {
            Expect.Call(_requestChannel.Send(_messageMock)).Return(false);
            _mocks.ReplayAll();
            _simpleMessagingGateway.Send(_messageMock);
            _mocks.VerifyAll();
        }

        [Test, Ignore("investigate rhino callback")]
        public void SendObject()
        {
            /*
            Expect.Call(_requestChannel.Send(null)).Callback(delegate(IMessage msg) { return true; }).Return(true);
            LastCall.IgnoreArguments().Constraints(Is.TypeOf<string>());
            _mocks.ReplayAll();
            _simpleMessagingGateway.Send("test");
            _mocks.VerifyAll();
             */
        }

        [Test] //, ExpectedException(typeof(MessageDeliveryException))]
        public void SendObjectFailure()
        {
            //expect(_requestChannel.send(isA(Message.class))).andAnswer(new IAnswer<Boolean>() {
            //    public Boolean answer() throws Throwable {
            //        assertEquals("test", ((Message) getCurrentArguments()[0]).getPayload());
            //        return false;
            //    }
            //});
            //replay(_allmocks);
            //this._simpleMessagingGateway.send("test");
            //verify(_allmocks);
        }

        [Test, ExpectedException(typeof (ArgumentNullException))] //(expected = IllegalArgumentException.class)
        public void SendMessageNull()
        {
            _mocks.ReplayAll();
            try
            {
                _simpleMessagingGateway.Send(null);
            }
            finally
            {
                _mocks.VerifyAll();
            }
        }

        ///* receive tests */
        [Test]
        public void ReceiveMessage()
        {
            Expect.Call(_replyChannel.Receive()).Return(_messageMock);
            Expect.Call(_messageMock.Payload).Return("test").Repeat.Any(); //.anyTimes();
            _mocks.ReplayAll();
            Assert.That(_simpleMessagingGateway.Receive(), NUnit.Framework.Is.EqualTo("test"));
            _mocks.VerifyAll();
        }

        [Test]
        public void ReceiveMessageNull()
        {
            Expect.Call(_replyChannel.Receive()).Return(null);
            _mocks.ReplayAll();
            Assert.IsNull(_simpleMessagingGateway.Receive());
            _mocks.VerifyAll();
        }

        ///* send and receive tests */
        [Test, Ignore("investigate rhino mocks")]
        public void SendObjectAndReceiveObject()
        {
            /*
            Expect.Call(_replyChannel.Name).Return("_replyChannel").Repeat.Any();
                                                                                      
            Expect.Call(_requestChannel.Send(null)).IgnoreArguments().Constraints(Is.TypeOf(typeof (string))).
                Constraints().Return(true);
            _mocks.ReplayAll();
            _simpleMessagingGateway.ReplyTimeout = TimeSpan.Zero;
            _simpleMessagingGateway.SendAndReceive("test");
            _mocks.VerifyAll();
             */
        }

        //@Test
        //@Ignore
        //public void sendMessageAndReceiveObject() {
        //    // setup local _mocks
        //    MessageHeaders messageHeadersMock = createMock(MessageHeaders.class);	
        //    //set expectations
        //    expect(_replyChannel.getName()).andReturn("_replyChannel").anyTimes();
        //    expect(_messageMock.getHeaders()).andReturn(messageHeadersMock);
        //    expect(_requestChannel.send(_messageMock)).andReturn(true);
        //    expect(messageHeadersMock.getId()).andReturn(1);

        //    //play scenario
        //    replay(_allmocks);
        //    replay(messageHeadersMock);
        //    this._simpleMessagingGateway.sendAndReceive(_messageMock);
        //    verify(_allmocks);
        //    verify(messageHeadersMock);
        //}

        //@Test(expected = IllegalArgumentException.class)
        //public void sendNullAndReceiveObject() {
        //    expect(_replyChannel.getName()).andReturn("_replyChannel").anyTimes();
        //    replay(_allmocks);
        //    try {
        //        this._simpleMessagingGateway.sendAndReceive(null);
        //    }
        //    finally {
        //        verify(_allmocks);
        //    }
        //}

        //@Test
        //public void sendObjectAndReceiveMessage() {
        //    expect(_replyChannel.getName()).andReturn("_replyChannel").anyTimes();
        //    expect(_requestChannel.send(isA(Message.class))).andReturn(true);
        //    replay(_allmocks);
        //    this._simpleMessagingGateway.setReplyTimeout(0);
        //    this._simpleMessagingGateway.sendAndReceiveMessage("test");
        //    verify(_allmocks);
        //}

        //@Test
        //@Ignore
        //public void sendMessageAndReceiveMessage() {
        //    // setup local _mocks
        //    MessageHeaders messageHeadersMock = createMock(MessageHeaders.class);	
        //    //set expectations
        //    expect(_replyChannel.getName()).andReturn("_replyChannel").anyTimes();
        //    expect(_messageMock.getHeaders()).andReturn(messageHeadersMock);
        //    expect(messageHeadersMock.getReplyChannel()).andReturn(_replyChannel);
        //    expect(_requestChannel.send(_messageMock)).andReturn(true);
        //    expect(messageHeadersMock.getId()).andReturn(1);

        //    replay(_allmocks);
        //    this._simpleMessagingGateway.sendAndReceiveMessage(_messageMock);
        //    verify(_allmocks);
        //}

        //@Test(expected = IllegalArgumentException.class)
        //public void sendNullAndReceiveMessage() {
        //    expect(_replyChannel.getName()).andReturn("_replyChannel").anyTimes();
        //    replay(_allmocks);
        //    try {
        //        this._simpleMessagingGateway.sendAndReceiveMessage(null);
        //    }
        //    finally {
        //        verify(_allmocks);
        //    }
        //}
    }
}