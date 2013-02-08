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
using Spring.Core;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Support.Converter;

namespace Spring.Integration.Nms
{
    /// <summary>
    ///  A MessageConsumer that sends the converted Message payload within a NMS Message.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsSendingMessageHandler : AbstractNmsTemplateBasedAdapter, IMessageHandler, IOrdered
    {
        private volatile bool extractPayload = true;

        private volatile int order = int.MaxValue; //LOWEST_PRECEDENCE

        /// <summary>
        /// Sets a value indicating whether to extract payload.  True by default.
        /// </summary>
        /// <value><c>true</c> if extract payload; otherwise, <c>false</c>.</value>
        public bool ExtractPayload
        {
            set { extractPayload = value; }
        }

        #region Overrides of AbstractNmsTemplateBasedAdapter

        protected override void ConfigureMessageConverter(NmsTemplate nmsTemplate, INmsHeaderMapper headerMapper)
        {
            IMessageConverter converter = nmsTemplate.MessageConverter;
            if (converter == null || !(converter is HeaderMappingMessageConverter))
            {
                HeaderMappingMessageConverter hmmc = new HeaderMappingMessageConverter(converter, headerMapper);
                hmmc.ExtractIntegrationMessagePayload = extractPayload;
                nmsTemplate.MessageConverter = hmmc;
            }
        }

        #endregion

        #region Implementation of IMessageHandler

        public void HandleMessage(IMessage message)
        {
            if (message == null)
            {
                throw new ArgumentException("message must not be null");
            }
            NmsTemplate.ConvertAndSend(message);
        }

        #endregion

        #region Implementation of IOrdered

        public int Order
        {
            get { return order; }
            set { order = value;}
        }

        #endregion
    }

}