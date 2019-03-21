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

using Spring.Threading;

#endregion

namespace Spring.Integration.Tests.Config
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class TestObject
    {
        private string _message;

        private readonly CountDownLatch _latch;

        private string _replyMessageText;


        public TestObject()
            : this(1)
        {
        }

        public TestObject(int countdown)
        {
            _latch = new CountDownLatch(countdown);
        }


        public string ReplyMessageText
        {
            set { _replyMessageText = value; }
        }

        public CountDownLatch Latch
        {
            get { return _latch; }
        }

        public string Store(string message)
        {
            _message = message;
            _latch.CountDown();
            return _replyMessageText;
        }

        public string GetMessage()
        {
            return _message;
        }
    }
}