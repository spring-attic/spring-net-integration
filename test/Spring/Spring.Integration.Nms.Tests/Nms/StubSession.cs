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
    public class StubSession : ISession
    {
        private string messageText;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public StubSession(string messageText)
        {
            this.messageText = messageText;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            
        }

        #endregion

        #region Implementation of ISession

        public IMessageProducer CreateProducer()
        {
            //TODO - is null ok here?
            return new StubProducer(null);
        }

        public IMessageProducer CreateProducer(IDestination destination)
        {
            return new StubProducer(destination);
        }

        public IMessageConsumer CreateConsumer(IDestination destination)
        {
            return new StubConsumer(messageText);
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector)
        {
            return new StubConsumer(messageText);
        }

        public IMessageConsumer CreateConsumer(IDestination destination, string selector, bool noLocal)
        {
            return new StubConsumer(messageText);
        }

        public IMessageConsumer CreateDurableConsumer(ITopic destination, string name, string selector, bool noLocal)
        {
            return null;
        }

        public void DeleteDurableConsumer(string name)
        {
            
        }

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue.
        /// </summary>
        /// <param name="queue">A <see cref="T:Apache.NMS.IQueue"/></param>
        /// <returns>
        /// A <see cref="T:Apache.NMS.IQueueBrowser"/>
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">If the Prodiver does not support creation of Queue Browsers.
        ///             </exception>
        public IQueueBrowser CreateBrowser(IQueue queue)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a QueueBrowser object to peek at the messages on the specified queue
        ///             using a message selector.
        /// </summary>
        /// <param name="queue">A <see cref="T:Apache.NMS.IQueue"/></param><param name="selector">A <see cref="T:System.String"/></param>
        /// <returns>
        /// A <see cref="T:Apache.NMS.IQueueBrowser"/>
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">If the Prodiver does not support creation of Queue Browsers.
        ///             </exception>
        public IQueueBrowser CreateBrowser(IQueue queue, string selector)
        {
            throw new NotImplementedException();
        }

        public IQueue GetQueue(string name)
        {
            return null;
        }

        public ITopic GetTopic(string name)
        {
            return null;
        }

        public ITemporaryQueue CreateTemporaryQueue()
        {
            return null;
        }

        public ITemporaryTopic CreateTemporaryTopic()
        {
            return null;
        }

        public void DeleteDestination(IDestination destination)
        {
            
        }

        public IMessage CreateMessage()
        {
            return null;
        }

        public ITextMessage CreateTextMessage()
        {
            return null;
        }

        public ITextMessage CreateTextMessage(string text)
        {
            return new StubTextMessage(text);
        }

        public IMapMessage CreateMapMessage()
        {
            return null;
        }

        public IObjectMessage CreateObjectMessage(object body)
        {
            return null;
        }

        public IBytesMessage CreateBytesMessage()
        {
            return null;
        }

        public IBytesMessage CreateBytesMessage(byte[] body)
        {
            return null;
        }

        /// <summary>
        /// Creates a new stream message
        /// </summary>
        public IStreamMessage CreateStreamMessage()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        /// <summary>
        /// A Delegate that is called each time a Message is dispatched to allow the client to do
        ///             any necessary transformations on the received message before it is delivered.
        ///             The Session instance sets the delegate on each Consumer it creates.
        /// </summary>
        public ConsumerTransformerDelegate ConsumerTransformer { get; set; }

        /// <summary>
        /// A delegate that is called each time a Message is sent from this Producer which allows
        ///             the application to perform any needed transformations on the Message before it is sent.
        ///             The Session instance sets the delegate on each Producer it creates.
        /// </summary>
        public ProducerTransformerDelegate ProducerTransformer { get; set; }

        public TimeSpan RequestTimeout
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Transacted
        {
            get { return false;}
        }

        public AcknowledgementMode AcknowledgementMode
        {
            get { return Apache.NMS.AcknowledgementMode.AutoAcknowledge; }
        }

        #endregion
    }
}