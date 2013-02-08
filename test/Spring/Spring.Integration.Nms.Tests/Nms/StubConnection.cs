#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using Apache.NMS;

namespace Spring.Integration.Nms
{
    /// <summary>
    ///  
    /// </summary>
    /// <author>Mark Pollack</author>
    public class StubConnection : IConnection
    {

        private string messageText;

        public StubConnection(string messageText)
        {
            this.messageText = messageText;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Implementation of IStartable

        public void Start()
        {
        }

        public bool IsStarted
        {
            get { return true; }
        }

        #endregion

        #region Implementation of IStoppable

        public void Stop()
        {
        }

        #endregion

        #region Implementation of IConnection

        public ISession CreateSession()
        {
            return new StubSession(messageText);
        }

        public ISession CreateSession(AcknowledgementMode acknowledgementMode)
        {
            return new StubSession(messageText);
        }

        public void Close()
        {

        }

        public TimeSpan RequestTimeout
        {
            get { return TimeSpan.FromMilliseconds(0); }
            set {  }
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { return Apache.NMS.AcknowledgementMode.AutoAcknowledge; }
            set {  }
        }

        public string ClientId
        {
            get { return null; }
            set {  }
        }

        public event ExceptionListener ExceptionListener;

        #endregion
    }
}