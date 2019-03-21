#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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
using Spring.Integration.Message;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Support.Converter;
using IMessage=Spring.Integration.Core.IMessage;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// A source for receiving NMS Messages with a polling listener. This source is
    /// only recommended for very low message volume. Otherwise, the
    /// <see cref="NmsMessageDrivenEndpoint"/> that uses Spring's MessageListener container
    /// support is a better option
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsDestinationPollingSource : AbstractNmsTemplateBasedAdapter, IMessageSource
    {

        private volatile bool extractPayload = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsDestinationPollingSource"/> class.
        /// </summary>
        /// <param name="nmsTemplate">The NMS template.</param>
        public NmsDestinationPollingSource(NmsTemplate nmsTemplate) : base(nmsTemplate)
        {
        }

        #region Overrides of AbstractNmsTemplateBasedAdapter

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsDestinationPollingSource"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="destination">The destination.</param>
        public NmsDestinationPollingSource(IConnectionFactory connectionFactory, IDestination destination) : base(connectionFactory, destination)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NmsDestinationPollingSource"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="destinationName">Name of the destination.</param>
        public NmsDestinationPollingSource(IConnectionFactory connectionFactory, string destinationName) : base(connectionFactory, destinationName)
        {
        }


        /// <summary>
        /// Sets a value indicating whether the playload should be extracted from each received NMS
        /// Message to be used as the Spring Integration Message payload.
        /// </summary>
        /// <remarks>
        /// The default value is true. To force creation of Spring Integration Messages whose 
        /// payload is the actual JMS Message, set this to false.
        /// </remarks>
        /// <value><c>true</c> to extract payload; otherwise, <c>false</c>.</value>
        public bool ExtractPayload
        {
            set { extractPayload = value; }
        }

        protected override void ConfigureMessageConverter(NmsTemplate nmsTemplate, INmsHeaderMapper headerMapper)
        {
            IMessageConverter converter = nmsTemplate.MessageConverter;
            if (converter == null || !(converter is HeaderMappingMessageConverter))
            {
                HeaderMappingMessageConverter hmmc = new HeaderMappingMessageConverter(converter, headerMapper);
                hmmc.ExtractNmsMessageBody = extractPayload;
                nmsTemplate.MessageConverter = hmmc;
            }
        }

        #endregion

        #region Implementation of IMessageSource

        public IMessage Receive()
        {
            object receivedObject = NmsTemplate.ReceiveAndConvert();
            if (receivedObject == null)
            {
                return null;
            }
            if (receivedObject is IMessage)
            {
                return (IMessage) receivedObject;
            }
            return new Spring.Integration.Message.Message(receivedObject);
        }

        #endregion
    }

}