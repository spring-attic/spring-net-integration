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
using Apache.NMS;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Messaging.Nms.Listener;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Messaging.Nms.Support.Destinations;
using Spring.Objects.Factory;
using Spring.Util;
using IMessage=Apache.NMS.IMessage;

#endregion

namespace Spring.Integration.Nms
{
    /// <summary>
    /// ActiveMQ (NMS) MessageListener that converts a NMS Message into a Spring Integration
    /// Message and sends that Message to a channel. If the 'expectReply' value is
    /// true, it will also wait for a Spring Integration reply Message
    /// and convert that into a NMS reply.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class ChannelPublishingJmsMessageListener : ISessionAwareMessageListener, IInitializingObject
    {
        private volatile bool expectReply;

        private volatile IMessageConverter messageConverter;

        private volatile bool extractRequestPayload = true;

        private volatile bool extractReplyPayload = true;

        private volatile Object defaultReplyDestination;

        private volatile IDestinationResolver destinationResolver = new DynamicDestinationResolver();

        private volatile INmsHeaderMapper headerMapper;

        private readonly MessageChannelTemplate channelTemplate = new MessageChannelTemplate();


        /// <summary>
        /// Specify the channel to which request Messages should be sent.
        /// </summary>
        /// <value>The request channel.</value>
        public IMessageChannel RequestChannel
        {
            set { this.channelTemplate.DefaultChannel = value; }
        }

        /// <summary>
        /// Sets a value indicating whether a NMS reply Message is expected.  Default is false.
        /// </summary>
        /// <value><c>true</c> if expect reply; otherwise, <c>false</c>.</value>
        public bool ExpectReply
        {
            set { expectReply = value; }
        }

        /// <summary>
        /// Set the maximum time to wait when sending a request message to the
        /// request channel. The default value will be that of IMessageChannelTemplate
        /// </summary>
        /// <value>The request timeout.</value>
        public TimeSpan RequestTimeout
        {
            set { this.channelTemplate.SendTimeout = value; }
        }

        /// <summary>
        /// Sets the maximum time to wait for reply Messages. This value is only
        /// relevant if ExpectReply is true. The default
        /// value will be that of MessageChannelTemplate.
        /// </summary>
        /// <value>The reply timeout.</value>
        public TimeSpan ReplyTimeout
        {
            set { this.channelTemplate.ReceiveTimeout = value; }
        }

        /// <summary>
        /// Sets the default reply destination to send reply messages to. This will
        /// be applied in case of a request message that does not carry a
        /// "NMSReplyTo" field.
        /// </summary>
        /// <value>The default reply destination.</value>
        public IDestination DefaultReplyDestination
        {
            set { defaultReplyDestination = value; }
        }

        /// <summary>
        /// Sets the default reply queue to send reply messages to.
	    /// This will be applied in case of a request message that does not carry a
	    /// "NMSReplyTo" field.
	    /// Alternatively, specify a JMS Destination object as "defaultReplyDestination".
        /// </summary>
        /// <value>The default name of the reply queue.</value>
        public String DefaultReplyQueueName
        {
            set { this.defaultReplyDestination = new DestinationNameHolder(value, false); }
        }


        /// <summary>
        /// Set the name of the default reply topic to send reply messages to.
	    /// This will be applied in case of a request message that does not carry a
	    /// "NMSReplyTo" field.
	    /// Alternatively, specify a JMS Destination object as "defaultReplyDestination".
        /// </summary>
        /// <value>The default name of the reply topic.</value>
        public String DefaultReplyTopicName
        {
            set { this.defaultReplyDestination = new DestinationNameHolder(value, false); }
        }


        /// <summary>
        /// Sets the DestinationResolver that should be used to resolve reply
	    /// destination names for this listener.	    
        /// </summary>
        /// <remarks>
        /// The default resolver is a DynamicDestinationResolver. 
        /// </remarks>
        /// <value>The destination resolver.</value>
        public IDestinationResolver DestinationResolver
        {
             set
             {
                 AssertUtils.ArgumentNotNull(destinationResolver, "destinationResolver must not be null");
                 this.destinationResolver = value;
             }
        }


        /// <summary>
        /// Sets the IMessageConverter implementation to use when
	    /// converting between NMS Messages and Spring Integration Messages. 
	    /// If none is provided, a HeaderMappingMessageConverter
	    /// be used and the  NmsHeaderMapper instance provided to the
	    /// HeaderMapper property will be included
	    /// in the conversion process.
        /// </summary>
        /// <value>The message converter.</value>
        public IMessageConverter MessageConverter
        {
            set
            {
                this.messageConverter = value;
            }
        }

        /// <summary>
        /// Sets the header mapper implementation to use when converting between NMS Messages and
        /// Spring Integration Messages.  If none is provided, a <see cref="DefaultNmsHeaderMapper"/> will be
        /// used.
        /// </summary>
        /// <remarks>
        /// his property will be ignored if a <see cref="IMessageConverter"/> is provided to the <see cref="MessageConverter"/>
        /// is provided to the MessageConverter property.  However, you may provide your own implementation of the 
        /// delegating <see cref="HeaderMappingMessageConverter"/> implementation.
        /// </remarks>
        /// <value>The header mapper.</value>
        public INmsHeaderMapper HeaderMapper
        {
            set { headerMapper = value; }
        }


        /// <summary>
        /// Sets whether the NMS request Message's body should be extracted prior
	    /// to converting into a Spring Integration Message. This value is set to
	    /// true by default. To send the NMS Message itself as a
	    /// Spring Integration Message payload, set this to false.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if extract request payload; otherwise, <c>false</c>.
        /// </value>
        public bool ExtractRequestPayload
        {
            set { extractRequestPayload = value; }
        }

        /// <summary>
        /// Sets whether the Spring Integration reply Message's payload should be
	    /// extracted prior to converting into a JMS Message. This value is set to
	    /// true by default. To send the Spring Integration Message 
	    /// itself as the NMS Message's body, set this to false.
        /// </summary>
        /// <value><c>true</c> if extract reply payload; otherwise, <c>false</c>.</value>
        public bool ExtractReplyPayload
        {
            set { extractReplyPayload = value; }
        }



        #region Implementation of ISessionAwareMessageListener

        public void OnMessage(IMessage nmsMessage, ISession session)
        {
            object obj = messageConverter.FromMessage(nmsMessage);
            Spring.Integration.Core.IMessage requestMessage = obj as Spring.Integration.Core.IMessage;
            if (requestMessage == null)
            {
                requestMessage = MessageBuilder.WithPayload(obj).Build();
            }
            if (!expectReply)
            {
                bool sent = channelTemplate.Send(requestMessage);
                if (!sent)
                {
                    throw new MessageDeliveryException(requestMessage, "failed to send Message to request channel");
                }
            } else
            {
                Spring.Integration.Core.IMessage replyMessage = channelTemplate.SendAndReceive(requestMessage);
                if (replyMessage != null)
                {
                    IDestination destination = GetReplyDestination(nmsMessage, session);
                    Apache.NMS.IMessage nmsReply = messageConverter.ToMessage(replyMessage, session);
                    if (nmsReply.NMSCorrelationID != null)
                    {
                        nmsReply.NMSCorrelationID = nmsMessage.NMSMessageId;
                    }
                    Apache.NMS.IMessageProducer producer = session.CreateProducer(destination);                  
                    try
                    {
                        producer.Send(nmsReply);
                    } finally
                    {
                        producer.Close();
                    }
                }
            }
            
        }

        #endregion

        #region Implementation of IInitializingObject

        public void AfterPropertiesSet()
        {
            HeaderMappingMessageConverter headerMappingMessageConverter =
                messageConverter as HeaderMappingMessageConverter;
            if (headerMappingMessageConverter == null)
            {
                HeaderMappingMessageConverter hmmc = new HeaderMappingMessageConverter(messageConverter, headerMapper);
                hmmc.ExtractNmsMessageBody = extractRequestPayload;
                hmmc.ExtractIntegrationMessagePayload = extractReplyPayload;
                messageConverter = hmmc;
            }
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Determine the reply destination for the given message
        /// </summary>
        /// <remarks>
        /// This implementation first checks the NMS ReplyTo Destination of the supplied request message; if that is 
        /// not null, it is returned; if it is null, then the return value of <see cref="ResolveDefaultReplyDestination"/> 
        /// is returned; ift his too is null, then an <see cref="InvalidDestinationException"/> if thrown
        /// </remarks>
        /// <param name="request">The original incoming NMS message.</param>
        /// <param name="session">The NMS session to operate on.</param>
        /// <returns>The reply destination (never null).</returns>
        /// <exception cref="InvalidDestinationException">If no <see cref="IDestination"/> can be determined.
        /// </exception>
        /// <seealso cref="DefaultReplyDestination"/>
        private IDestination GetReplyDestination(Apache.NMS.IMessage request, Apache.NMS.ISession session)
        {
            IDestination replyTo = request.NMSReplyTo;
            if (replyTo == null)
            {
                replyTo = ResolveDefaultReplyDestination(session);
                if (replyTo == null)
                {
                    throw new InvalidDestinationException("Cannot determine reply destination: " +
                        "Request message does not contain reply-to destination, and no default reply destination set.");
                }
            }
            return replyTo;
        }

        private IDestination ResolveDefaultReplyDestination(ISession session)
        {
            IDestination d = defaultReplyDestination as IDestination;
            if (defaultReplyDestination != null)
            {
                return d;
            }
            DestinationNameHolder dnh = defaultReplyDestination as DestinationNameHolder;
            if (dnh != null)
            {
                return destinationResolver.ResolveDestinationName(session, dnh.name, dnh.isTopic);
            }
            return null;
        }

        #endregion

        private class DestinationNameHolder
        {
            public readonly String name;

            public readonly bool isTopic;

            public DestinationNameHolder(String name, bool isTopic)
            {
                this.name = name;
                this.isTopic = isTopic;
            }
        }
 
    }

}