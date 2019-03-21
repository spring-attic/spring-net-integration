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

#region

using System;
using Apache.NMS;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Messaging.Nms.Connections;
using Spring.Messaging.Nms.Support;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Messaging.Nms.Support.Destinations;
using Spring.Objects.Factory;
using Spring.Util;
using IMessage=Spring.Integration.Core.IMessage;

#endregion

namespace Spring.Integration.Nms
{
    /// <summary>
    /// An outbound Messaging Gateway for request/reply JMS.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsOutboundGateway : AbstractReplyProducingMessageHandler, IInitializingObject
    {
        private volatile IDestination requestDestination;

        private volatile String requestDestinationName;

        private volatile IDestination replyDestination;

        private volatile String replyDestinationName;

        private volatile IDestinationResolver destinationResolver = new DynamicDestinationResolver();

        private volatile bool pubSubDomain;

        private TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(5000);

        private volatile MsgDeliveryMode deliveryMode = NMSConstants.defaultDeliveryMode;

        private TimeSpan timeToLive = NMSConstants.defaultTimeToLive;

        private volatile MsgPriority priority = NMSConstants.defaultPriority;

        private IConnectionFactory connectionFactory;

        private volatile IMessageConverter messageConverter;

        private volatile INmsHeaderMapper headerMapper;

        private volatile bool extractRequestPayload = true;

        private volatile bool extractReplyPayload = true;

        private volatile bool initialized;

        private readonly object initializationMonitor = new Object();

        public IConnectionFactory ConnectionFactory
        {
            set { connectionFactory = value; }
        }

        public IDestination RequestDestination
        {
            set
            {
                if (value is ITopic)
                {
                    pubSubDomain = true;
                }
                requestDestination = value;
            }
        }

        public string RequestDestinationName
        {
            set { requestDestinationName = value; }
        }

        public IDestination ReplyDestination
        {
            set { replyDestination = value; }
        }

        public string ReplyDestinationName
        {
            set { replyDestinationName = value; }
        }

        public IDestinationResolver DestinationResolver
        {
            set { destinationResolver = value; }
        }

        public bool PubSubDomain
        {
            set { pubSubDomain = value; }
        }

        public TimeSpan ReceiveTimeout
        {
            set { receiveTimeout = value; }
        }

        public MsgDeliveryMode DeliveryMode
        {
            set { deliveryMode = value; }
        }

        public TimeSpan TimeToLive
        {
            set { timeToLive = value; }
        }

        public MsgPriority Priority
        {
            set { priority = value; }
        }

        public IMessageConverter MessageConverter
        {
            set
            {
                AssertUtils.ArgumentNotNull(value, "'messageConverter' must not be null");
                messageConverter = value;
            }
        }

        public INmsHeaderMapper HeaderMapper
        {
            set { headerMapper = value; }
        }

        public bool ExtractRequestPayload
        {
            set { extractRequestPayload = value; }
        }

        public bool ExtractReplyPayload
        {
            set { extractReplyPayload = value; }
        }

        private IDestination GetRequestDestination(ISession session)
        {
            if (requestDestination != null)
            {
                return requestDestination;
            }
            AssertUtils.ArgumentNotNull(this.destinationResolver,
                                        "DestinationResolver is required when relying upon the 'requestDestinationName' property.");
            return this.destinationResolver.ResolveDestinationName(session, requestDestinationName, pubSubDomain);
        }

        private IDestination GetReplyDestination(ISession session)
        {
            if (replyDestination != null)
            {
                return replyDestination;
            }
            if (replyDestinationName != null)
            {
                AssertUtils.ArgumentNotNull(this.destinationResolver,
                                            "DestinationResolver is required when relying upon the 'replyDestinationName' property.");
                return destinationResolver.ResolveDestinationName(
                    session, replyDestinationName, pubSubDomain);
            }
            return session.CreateTemporaryQueue();
        }

        #region Overrides of AbstractReplyProducingMessageHandler

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyMessageHolder)
        {
            if (!initialized)
            {
                AfterPropertiesSet();
            }
            IMessage requestMessage = MessageBuilder.FromMessage(message).Build();
            try
            {
                Apache.NMS.IMessage nmsReply = SendAndReceive(requestMessage);
            }
            catch (NMSException ex)
            {
                throw new MessageHandlingException(requestMessage, ex);
            }
        }

        private Apache.NMS.IMessage SendAndReceive(IMessage requestMessage)
        {
            IConnection connection = CreateConnection();
            ISession session = null;
            IMessageProducer messageProducer = null;
            IMessageConsumer messageConsumer = null;
            IDestination replyTo = null;
            try
            {
                session = CreateSession(connection);
                Apache.NMS.IMessage jmsRequest = this.messageConverter.ToMessage(requestMessage, session);
                messageProducer = session.CreateProducer(this.GetRequestDestination(session));
                messageProducer.DeliveryMode = deliveryMode;
                messageProducer.Priority = priority;
                messageProducer.TimeToLive = timeToLive;
                replyTo = GetReplyDestination(session);
                jmsRequest.NMSReplyTo = replyTo;
                connection.Start();
                messageProducer.Send(jmsRequest);
                if (replyTo is ITemporaryQueue || replyTo is ITemporaryTopic)
                {
                    messageConsumer = session.CreateConsumer(replyTo);
                }
                else
                {
                    String messageId = jmsRequest.NMSMessageId.Replace("'", "''");
                    String messageSelector = "NMSCorrelationID = '" + messageId + "'";
                    messageConsumer = session.CreateConsumer(replyTo, messageSelector);
                }
                return (TimeSpan.Compare(receiveTimeout, TimeSpan.FromMilliseconds(0)) == 1)
                           ? messageConsumer.Receive(receiveTimeout)
                           : messageConsumer.Receive();
            }
            finally
            {
                NmsUtils.CloseMessageProducer(messageProducer);
                NmsUtils.CloseMessageConsumer(messageConsumer);
                this.DeleteDestinationIfTemporary(session, replyTo);
                NmsUtils.CloseSession(session);

                ConnectionFactoryUtils.ReleaseConnection(connection, this.connectionFactory, true);
            }
        }

        protected virtual void DeleteDestinationIfTemporary(ISession session, IDestination destination)
        {
            try
            {
                if (destination is ITemporaryQueue || destination is ITemporaryTopic)
                {
                    session.DeleteDestination(destination);
                }
            }
            catch (NMSException)
            {
                // ignore
            }
        }

        protected virtual ISession CreateSession(IConnection connection)
        {
            return connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
        }

        protected virtual IConnection CreateConnection()
        {
            return this.connectionFactory.CreateConnection();
        }

        #endregion

        #region Implementation of IInitializingObject

        public void AfterPropertiesSet()
        {
            lock (initializationMonitor)
            {
                if (initialized)
                {
                    return;
                }
                AssertUtils.ArgumentNotNull(connectionFactory, "connectionFactory must not be null");
                AssertUtils.IsTrue(requestDestination != null || requestDestinationName != null,
                                   "Either a 'requestDestination' or 'requestDestinationName' is required.");
                if (messageConverter == null)
                {
                    HeaderMappingMessageConverter hmmc = new HeaderMappingMessageConverter(null, headerMapper);
                    hmmc.ExtractIntegrationMessagePayload = extractRequestPayload;
                    hmmc.ExtractNmsMessageBody = extractReplyPayload;
                    messageConverter = hmmc;
                }
                initialized = true;
            }
        }

        #endregion
    }
}