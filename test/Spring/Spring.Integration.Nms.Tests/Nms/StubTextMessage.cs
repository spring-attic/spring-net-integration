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
using Apache.NMS.ActiveMQ.OpenWire;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// Stub implementation of ITextMessage for use in Unit Tests
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class StubTextMessage : ITextMessage
    {
        #region Fields

        private String text;

        private IDestination replyTo;

        private String messageId;

        private String correlationId;

        private String type;

        private IPrimitiveMap properties = new PrimitiveMap();
        #endregion


        public StubTextMessage()
        {
        }

        public StubTextMessage(string text)
        {
            this.text = text;
        }

        #region Implementation of IMessage

        public void Acknowledge()
        {
            
        }

        public IPrimitiveMap Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        public string NMSCorrelationID
        {
            get { return correlationId; }
            set { correlationId = value; }
        }

        public IDestination NMSDestination
        {
            get { return null; }
        }

        public TimeSpan NMSTimeToLive
        {
            get { return TimeSpan.Zero; }
            set {  }
        }

        public string NMSMessageId
        {
            get { return messageId; }
            set { messageId = value;}
        }

        public MsgDeliveryMode NMSDeliveryMode
        {
            get { return MsgDeliveryMode.Persistent; }
            set {  }
        }

        public MsgPriority NMSPriority
        {
            get { return MsgPriority.Normal; }
            set {  }
        }

        public bool NMSRedelivered
        {
            get { return false; }
        }

        public IDestination NMSReplyTo
        {
            get { return replyTo; }
            set { replyTo = value; }
        }

        public DateTime NMSTimestamp
        {
            get { return DateTime.Today; }
        }

        public string NMSType
        {
            get { return type; }
            set { type = value; }
        }

        #endregion

        #region Implementation of ITextMessage

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        #endregion
    }
}