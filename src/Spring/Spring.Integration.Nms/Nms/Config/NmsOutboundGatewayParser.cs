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
using System.Xml;
using Spring.Integration.Config.Xml;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Nms.Config
{
    /// <summary>
    ///  
    /// </summary>
    /// <author>Mark Pollack</author>
    public class NmsOutboundGatewayParser : AbstractConsumerEndpointParser
    {
        protected override string InputChannelAttributeName
        {
            get { return "request-channel"; }
        }

        #region Overrides of AbstractConsumerEndpointParser

        protected override ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(NmsOutboundGateway));
            builder.AddPropertyReference("connectionFactory", element.GetAttribute("connection-factory"));
            String requestDestination = element.GetAttribute("request-destination");
            String requestDestinationName = element.GetAttribute("request-destination-name");
            if (!(StringUtils.HasText(requestDestination) ^ StringUtils.HasText(requestDestinationName)))
            {
                parserContext.ReaderContext.ReportException(element, "request-destination or request-destination-name",
                        "Exactly one of the 'request-destination' or 'request-destination-name' attributes is required.");
            }
            if (StringUtils.HasText(requestDestination))
            {
                builder.AddPropertyReference("requestDestination", requestDestination);
            }
            else if (StringUtils.HasText(requestDestinationName))
            {
                builder.AddPropertyValue("requestDestinationName", requestDestinationName);
            }
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "reply-destination");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "reply-destination-name");
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "reply-channel");
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "message-converter");
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "header-mapper");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "extract-request-payload");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "extract-reply-payload");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "receive-timeout");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "pub-sub-domain");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "delivery-mode");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "time-to-live");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "priority");



            return builder;
            
        }

        #endregion
    }

}