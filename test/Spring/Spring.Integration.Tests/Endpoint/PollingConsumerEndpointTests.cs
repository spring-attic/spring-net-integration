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
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Message;
using Spring.Integration.Scheduling;
using Spring.Integration.Tests.Util;
using Spring.Threading;
using Spring.Threading.AtomicTypes;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Endpoint
{
    /// <author>Iwein Fuld</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    [TestFixture]
    public class PollingConsumerEndpointTests
    {
        private readonly MockRepository _mocks = new MockRepository();

        private PollingConsumer _endpoint;

        private readonly TestTrigger _trigger = new TestTrigger();

        private readonly TestConsumer _consumer = new TestConsumer();

        private readonly IMessage _message = new StringMessage("test");

        private IMessage _badMessage = new StringMessage("bad");

        private readonly TestErrorHandler _errorHandler = new TestErrorHandler();

        private IPollableChannel _channelMock; // = createMock(PollableChannel.class);

        private readonly SimpleTaskScheduler _taskScheduler = (SimpleTaskScheduler) TestUtils.CreateTaskScheduler(1);
                                             //new SimpleAsyncTaskExecutor());


        [SetUp]
        public void Init()
        {
            _consumer.Counter.Value = 0;
            _trigger.Reset();
            _channelMock = _mocks.StrictMock<IPollableChannel>();
            _endpoint = new PollingConsumer(_channelMock, _consumer);
            _endpoint.TaskScheduler = _taskScheduler;
            _taskScheduler.ErrorHandler = _errorHandler;
            _taskScheduler.Start();
            _endpoint.Trigger = _trigger;
            _endpoint.ReceiveTimeout = TimeSpan.FromMilliseconds(-1);
            //reset(_channelMock);
        }

        [TearDown]
        public void Stop()
        {
            _taskScheduler.Stop();
        }


        //[Test]
        //public void SingleMessage() {

        //    Expect.Call(_channelMock.Receive()).Return(_message);
        //    //expectLastCall();
        //    _mocks.Replay(_channelMock);
        //    _endpoint.MaxMessagesPerPoll = 1;
        //    _endpoint.Start();
        //    _trigger.Await();
        //    _endpoint.Stop();
        //    Assert.That(_consumer.Counter.IntegerValue, Is.EqualTo(1));
        //    _mocks.Verify(_channelMock);
        //}

        //[Test]
        //public void MultipleMessages() {
        //    Expect.Call(_channelMock.Receive()).Return(_message).times(5);
        //    replay(_channelMock);
        //    _endpoint.setMaxMessagesPerPoll(5);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    assertEquals(5, _consumer.counter.get());
        //    verify(_channelMock);
        //}

        //@Test
        //public void multipleMessages_underrun() {
        //    expect(_channelMock.receive()).andReturn(_message).times(5);
        //    expect(_channelMock.receive()).andReturn(null);
        //    replay(_channelMock);
        //    _endpoint.setMaxMessagesPerPoll(6);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    assertEquals(5, _consumer.counter.get());
        //    verify(_channelMock);
        //}

        //@Test(expected = MessageRejectedException.class)
        //public void rejectedMessage() throws Throwable {
        //    expect(_channelMock.receive()).andReturn(_badMessage);
        //    replay(_channelMock);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    verify(_channelMock);
        //    assertEquals(1, _consumer.counter.get());
        //    _errorHandler.throwLastErrorIfAvailable();
        //}

        //@Test(expected = MessageRejectedException.class)
        //public void droppedMessage_onePerPoll() throws Throwable {
        //    expect(_channelMock.receive()).andReturn(_badMessage).times(1);
        //    replay(_channelMock);
        //    _endpoint.setMaxMessagesPerPoll(10);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    verify(_channelMock);
        //    assertEquals(1, _consumer.counter.get());
        //    _errorHandler.throwLastErrorIfAvailable();
        //}

        //@Test
        //public void blockingSourceTimedOut() {
        //    // we don't need to await the timeout, returning null suffices
        //    expect(_channelMock.receive(1)).andReturn(null);
        //    replay(_channelMock);
        //    _endpoint.setReceiveTimeout(1);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    assertEquals(0, _consumer.counter.get());
        //    verify(_channelMock);
        //}

        //@Test
        //public void blockingSourceNotTimedOut() {
        //    expect(_channelMock.receive(1)).andReturn(_message);
        //    expectLastCall();
        //    replay(_channelMock);
        //    _endpoint.setReceiveTimeout(1);
        //    _endpoint.setMaxMessagesPerPoll(1);
        //    _endpoint.start();
        //    _trigger.await();
        //    _endpoint.stop();
        //    assertEquals(1, _consumer.counter.get());
        //    verify(_channelMock);
        //}


        private class TestConsumer : IMessageHandler
        {
            private volatile AtomicInteger _counter = new AtomicInteger();

            public AtomicInteger Counter
            {
                get { return _counter; }
            }

            public void HandleMessage(IMessage message)
            {
                _counter.IncrementValueAndReturn();
                if ("bad".Equals(message.Payload.ToString()))
                {
                    throw new MessageRejectedException(message, "intentional test failure");
                }
            }
        }


        private class TestTrigger : ITrigger
        {
            private readonly AtomicBoolean _hasRun = new AtomicBoolean();

            private volatile CountDownLatch _latch = new CountDownLatch(1);


            public DateTime GetNextRunTime(DateTime lastScheduledRunTime, DateTime lastCompleteTime)
            {
                if (!_hasRun.Exchange(true))
                {
                    return new DateTime();
                }
                _latch.CountDown();

                return default(DateTime);
            }

            public void Reset()
            {
                _latch = new CountDownLatch(1);
                _hasRun.Value = false;
            }

            public void Await()
            {
                try
                {
                    _latch.Await(TimeSpan.FromMilliseconds(5000));
                    if (_latch.Count != 0)
                    {
                        throw new SystemException("test latch.await() did not count down");
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    throw new SystemException("test latch.await() interrupted");
                }
            }
        }


        private class TestErrorHandler : IErrorHandler
        {
            private volatile Exception _lastError;

            public void HandleError(Exception t)
            {
                _lastError = t;
            }

            public void ThrowLastErrorIfAvailable()
            {
                Exception e = _lastError;
                _lastError = null;
                throw e;
            }
        }
    }
}