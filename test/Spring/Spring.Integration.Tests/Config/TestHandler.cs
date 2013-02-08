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

using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Threading;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class TestHandler
    {
        private string _messageString;

        private readonly CountDownLatch _latch;

        private string _replyMessageText;


        public TestHandler()
            : this(1)
        {
        }

        public TestHandler(int countdown)
        {
            _latch = new CountDownLatch(countdown);
        }


        public string ReplyMessageText
        {
            set { _replyMessageText = value; }
        }

        [ServiceActivator]
        public IMessage Handle(IMessage message)
        {
            _messageString = message.Payload.ToString();
            _latch.CountDown();
            return (_replyMessageText != null) ? new StringMessage(_replyMessageText) : null;
        }

        public string MessageString
        {
            get { return _messageString; }
        }

        public CountDownLatch Latch
        {
            get { return _latch; }
        }
    }
}