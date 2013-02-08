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

using System;
using System.Collections;
using System.Collections.Generic;
using Apache.NMS;
using Common.Logging;
using Spring.Integration.Adapter;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// Default implementation of <see cref="INmsHeaderMapper"/>.
    /// </summary>
    /// <remarks>
    /// This implementation copies NMS API headers (e.g. NMSReplyTo) to and from
    /// Spring Integration Messages. Any user-defined properties will also be copied
    /// from a NMS Message to a Spring Integration Message, and any other headers
    /// on a Spring Integration Message (beyond the NMS API headers) will likewise
    /// be copied to a NMS Message. Those other headers will be copied to the
    /// general properties of a NMS Message whereas the NMS API headers are passed
    /// to the appropriate setter methods (e.g. NMSReplyTo).
    /// <p/>
    /// Constants for the NMS API headers are defined in {@link JmsHeaders}.
    /// Note that the NMSMessageID and NMSRedelivered flag are only copied
    /// <em>from</em> a JMS Message. Those values will <em>not</em> be passed
    /// along from a Spring Integration Message to an outbound JMS Message.
    /// </remarks>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class DefaultNmsHeaderMapper : INmsHeaderMapper
    {
        /*
        private readonly static List<Type> SUPPORTED_PROPERTY_TYPES = new List<Type>();

        static DefaultNmsHeaderMapper()
        {
            lock (SUPPORTED_PROPERTY_TYPES)
            {
                SUPPORTED_PROPERTY_TYPES.Add(typeof (bool));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (byte));
                SUPPORTED_PROPERTY_TYPES.Add(typeof(char));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (double));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (float));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (int));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (long));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (short));
                SUPPORTED_PROPERTY_TYPES.Add(typeof (string));           
            }
        }*/

        private readonly ILog logger = LogManager.GetLogger(typeof (DefaultNmsHeaderMapper));

        #region Implementation of INmsHeaderMapper

        public void FromHeaders(MessageHeaders headers, Apache.NMS.IMessage nmsMessage)
        {
            try
            {
                if (headers.ContainsKey(NmsHeaders.CORRELATION_ID))
                {
                    object nmsCorrelationId = headers[NmsHeaders.CORRELATION_ID] as string;
                    if (nmsCorrelationId != null)
                    {
                        nmsMessage.NMSCorrelationID = (string) nmsCorrelationId;
                    }
                }
                if (headers.ContainsKey(NmsHeaders.REPLY_TO))
                {
                    object nmsReplyTo = headers[NmsHeaders.REPLY_TO] as IDestination;
                    if (nmsReplyTo != null)
                    {
                        nmsMessage.NMSReplyTo = (IDestination) nmsReplyTo;
                    }
                }
                if (headers.ContainsKey(NmsHeaders.TYPE))
                {
                    object nmsType = headers[NmsHeaders.TYPE] as string;
                    if (nmsType != null)
                    {
                        nmsMessage.NMSType = (string) nmsType;
                    }
                }
                ICollection<string> attributeNames = headers.Keys;                
                foreach (string attributeName in attributeNames)
                {
                    if (!attributeName.StartsWith(NmsHeaders.PREFIX))
                    {
                        if (StringUtils.HasText(attributeName))
                        {
                            object val = headers[attributeName];
                            if (val != null && IsValidPropertyType(val))
                            {
                                try
                                {
                                    nmsMessage.Properties[attributeName] = val;
                                }
                                catch (Exception ex)
                                {
                                    if (logger.IsWarnEnabled)
                                    {
                                        logger.Warn("failed to map Message header '"
                                                    + attributeName + "' to NMS property", ex);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                if (logger.IsWarnEnabled)
                {
                    logger.Warn("error occurred while mapping properties from MessageHeaders", ex);
                }
            }
        }

        public IDictionary<string, object> ToHeaders(Apache.NMS.IMessage nmsMessage)
        {
            IDictionary<string, object> headers = new Dictionary<string, object>();
            try
            {
                string messageId = nmsMessage.NMSMessageId;
                if (messageId != null)
                {
                    headers.Add(NmsHeaders.MESSAGE_ID, messageId);
                }
                string correlationId = nmsMessage.NMSCorrelationID;
                if (correlationId != null)
                {
                    headers.Add(NmsHeaders.CORRELATION_ID, correlationId);
                }
                IDestination replyTo = nmsMessage.NMSReplyTo;
                if (replyTo != null)
                {
                    headers.Add(NmsHeaders.REPLY_TO, replyTo);
                }
                headers.Add(NmsHeaders.REDELIVERED, nmsMessage.NMSRedelivered);
                string type = nmsMessage.NMSType;
                if (type != null)
                {
                    headers.Add(NmsHeaders.TYPE, type);
                }
                ICollection nmsPropertyNames = nmsMessage.Properties.Keys;
                if (nmsPropertyNames != null)
                {
                    foreach (string propertyName in nmsPropertyNames)
                    {
                        try
                        {
                            headers.Add(propertyName, nmsMessage.Properties[propertyName]);
                        } catch (Exception ex)
                        {
                            if (logger.IsWarnEnabled)
                            {
                                logger.Warn("error occurred while mapping NMS property '"
                                    + propertyName + "' to Message header", ex);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                throw new MessageMappingException("failure occurred while mapping NMS properties to MessageHeaders", ex);
            }
            return headers;
        }

        #endregion


        protected bool IsValidPropertyType(object val)
        {
            if (((val != null) && !(val is IList)) && !(val is IDictionary))
            {
                Type type = val.GetType();
                if (!((type.IsPrimitive || type.IsValueType) || type.IsAssignableFrom(typeof(string))))
                {
                    return false;                    
                }
                return true;
            }
            return false;
        }
    }

}