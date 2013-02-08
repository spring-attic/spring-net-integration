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
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using System;
using System.Collections.Generic;
using Apache.NMS;
using Common.Logging;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Messaging.Nms.Support.Converter;
using IMessage=Spring.Integration.Core.IMessage;

#endregion

namespace Spring.Integration.Nms
{
    /// <summary>
    /// A <see cref="IMessageConverter"/> implementation that is capable of delegating to
    /// an existing converter instance and an existing <see cref="INmsHeaderMapper"/>. The
    /// default IMessageConverter implementation is <see cref="SimpleMessageConverter"/>,
    /// and the default header mapper implementation is <see cref="DefaultNmsHeaderMapper"/>
    /// <summary/>
    /// <remarks>
    /// <para>If 'ExtractJmsMessageBody' is <c>true</c> (the default), the body
    /// of each received NMS Message will become the payload of a Spring Integration
    /// Message. Otherwise, the NMS Message itself will be the payload of the Spring
    /// Integration Message.</para>
    /// 
    /// <para>If 'ExtractIntegrationMessagePayload' is <c>true</c> (the default),
    /// the payload of each outbound Spring Integration Message will be passed to
    /// the IMessageConverter to produce the body of the NMS Message. Otherwise, the
    /// Spring Integration Message itself will become the body of the NMS Message.</para>
    /// 
    /// <para>The <see cref="INmsHeaderMapper"/> will be applied regardless of the values
    /// specified for Message extraction.
    /// </para>
    /// </remarks>
    /// 
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class HeaderMappingMessageConverter : IMessageConverter
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof (HeaderMappingMessageConverter));

        private readonly IMessageConverter converter;

        private readonly INmsHeaderMapper headerMapper;

        private volatile bool extractNmsMessageBody = true;

        private volatile bool extractIntegrationMessagePayload = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderMappingMessageConverter"/> class
        /// that will rely on the default <see cref="SimpleMessageConverter"/> and 
        /// <see cref="DefaultNmsHeaderMapper"/>.
        /// </summary>
        public HeaderMappingMessageConverter() : this(null, null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderMappingMessageConverter"/> class.
        /// that will delegate to the provided <see cref="IMessageConverter"/> instance and will use the default
        /// implementation of the <see cref="INmsHeaderMapper"/> strategy.
        /// </summary>
        /// <param name="converter">The converter.</param>
        public HeaderMappingMessageConverter(IMessageConverter converter)  :  this(converter, null)
        {
         
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderMappingMessageConverter"/> class.
        /// that will delegate to
        /// the provided <see cref="INmsHeaderMapper"/> instance and will use the default
        /// <see cref="SimpleMessageConverter"/> implementation.
        /// </summary>
        /// <param name="headerMapper">The header mapper.</param>
        public HeaderMappingMessageConverter(INmsHeaderMapper headerMapper) :  this(null, headerMapper)
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public HeaderMappingMessageConverter(IMessageConverter converter, INmsHeaderMapper headerMapper)
        {
            this.converter = (converter != null ? converter : new SimpleMessageConverter());
            this.headerMapper = (headerMapper != null ? headerMapper : new DefaultNmsHeaderMapper());
        }

        /// <summary>
        /// Sets a value indicating whether the inbound NMS Message's body should be extracted
        /// during the conversion process. Otherwise, the raw NMS Message itself
        /// will be the payload of the created Spring Integration Message. The
        /// HeaderMapper will be applied to the Message regardless of this value.
        /// </summary>
        /// <remarks>The default value is <c>true</c></remarks>
        /// <value>
        /// 	<c>true</c> if extract NMS message body; otherwise, <c>false</c>.
        /// </value>
        public bool ExtractNmsMessageBody
        {
            set { extractNmsMessageBody = value; }
        }

        /// <summary>
        /// Sets a value indicating whether the outbound integration Message's payload should be
        /// extracted prior to conversion into a JMS Message
        /// Otherwise, the
        /// integration Message itself will be passed to the converter.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if extract integration message payload; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// <para>Typically, this setting should be determined by the expectations of
        /// the target system. If the target system is not capable of understanding
        /// a Spring Integration Message, then set this to <code>true</code>.
        /// On the other hand, if the system is not only capable of understanding a
        /// Spring Integration Message but actually expected to rely upon Spring
        /// Integration Message Header values, then this must be set to
        /// <c>false</c> to ensure that the actual Message will be passed
        /// along with its Serializable headers.</para>
        /// <para>The default value is <c>true</c>
        public bool ExtractIntegrationMessagePayload
        {
            set { extractIntegrationMessagePayload = value; }
        }

        #region Implementation of IMessageConverter

        /// <summary>
        /// Convert from a NMS Message to a .NET object.
        /// </summary>
        /// <param name="nmsMessage">the message to convert</param>
        /// <returns>the converted .NET object</returns>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public object FromMessage(Apache.NMS.IMessage nmsMessage)
        {
            MessageBuilder builder = null;
            if (extractNmsMessageBody)
            {
                object conversionResult = converter.FromMessage(nmsMessage);
                if (conversionResult == null)
                {
                    return null;
                }
                if (conversionResult is Spring.Integration.Core.IMessage)
                {
                    builder = MessageBuilder.FromMessage((Spring.Integration.Core.IMessage)conversionResult);                    
                } else
                {
                    builder = MessageBuilder.WithPayload(conversionResult);
                }
            } else
            {
                builder = MessageBuilder.WithPayload(nmsMessage);
            }
            IDictionary<string, object> headers = headerMapper.ToHeaders(nmsMessage);
            Spring.Integration.Core.IMessage message = builder.CopyHeadersIfAbsent(headers).Build();
            if (logger.IsDebugEnabled)
            {
                logger.Debug("Converted NMS Message [" + nmsMessage + "] to integration message [" + message + "]");
            }
            return message;
            
        }

        /// <summary>
        /// Converts from an integration Message to a NMS Message.
        /// </summary>
        /// <param name="objectToConvert">the object to convert</param>
        /// <param name="session">the Session to use for creating a NMS Message</param>
        /// <returns>the NMS Message</returns>
        /// <throws>NMSException if thrown by NMS API methods </throws>
        /// <throws>MessageConversionException in case of conversion failure </throws>
        public Apache.NMS.IMessage ToMessage(object objectToConvert, ISession session)
        {
            MessageHeaders headers = null;
            Apache.NMS.IMessage nmsMessage = null;
            if (objectToConvert is Spring.Integration.Core.IMessage)
            {
                headers = ((Spring.Integration.Core.IMessage) objectToConvert).Headers;
                if (extractIntegrationMessagePayload)
                {
                    objectToConvert = ((IMessage) objectToConvert).Payload;
                }
            }
            nmsMessage = converter.ToMessage(objectToConvert, session);
            if (headers != null)
            {
                headerMapper.FromHeaders(headers, nmsMessage);
            }
            if (logger.IsDebugEnabled)
            {
                logger.Debug("converted [" + objectToConvert + "] to NMS Message [" + nmsMessage + "]");
            }
            return nmsMessage;
        }



        #endregion
    }
}