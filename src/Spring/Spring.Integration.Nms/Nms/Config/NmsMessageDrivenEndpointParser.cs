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

#region

using System;
using System.Xml;
using Apache.NMS;
using Spring.Integration.Config.Xml;
using Spring.Messaging.Nms.Listener;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

#endregion

namespace Spring.Integration.Nms.Config
{
    /// <summary>
    /// Parser for the &lt;outbound-gateway&gt; element of the integration 'nms' namespace.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public class NmsMessageDrivenEndpointParser : AbstractSingleObjectDefinitionParser
    {
        private static readonly string DEFAULT_REPLY_DESTINATION_ATTRIB = "default-reply-destination";

        private static readonly string DEFAULT_REPLY_QUEUE_NAME_ATTRIB = "default-reply-queue-name";

        private static readonly string DEFAULT_REPLY_TOPIC_NAME_ATTRIB = "default-reply-topic-name";

        private static readonly string[] containerAttributes = new[]
                                                                   {
                                                                       NmsAdapterParserUtils.CONNECTION_FACTORY_PROPERTY
                                                                       ,
                                                                       NmsAdapterParserUtils.DESTINATION_ATTRIBUTE,
                                                                       NmsAdapterParserUtils.DESTINATION_NAME_ATTRIBUTE,
                                                                       "destination-resolver", "transaction-manager",
                                                                       "pub-sub-domain",
                                                                       "concurrent-consumers",
                                                                       "max-concurrent-consumers",
                                                                       "max-messages-per-task",
                                                                       "idle-task-execution-limit"
                                                                   };

        private bool expectReply;

        public bool ExpectReply
        {
            set { expectReply = value; }
        }

        public NmsMessageDrivenEndpointParser(bool expectReply)
        {
            this.expectReply = expectReply;
        }

        protected override string GetObjectTypeName(XmlElement element)
        {
            return typeof (NmsMessageDrivenEndpoint).FullName;// +", " + typeof(NmsMessageDrivenEndpoint).Module;                       
        }

        protected override bool ShouldGenerateId
        {
            get { return false; }
        }

        protected override bool ShouldGenerateIdAsFallback
        {
            get { return true; }
        }

        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder)
        {
            String containerBeanName = ParseMessageListenerContainer(element, parserContext);
            String listenerBeanName = ParseMessageListener(element, parserContext);
            builder.AddConstructorArgReference(containerBeanName);
            builder.AddConstructorArgReference(listenerBeanName);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "auto-startup");
        }

        private string ParseMessageListenerContainer(XmlElement element, ParserContext parserContext)
        {
            if (element.HasAttribute("container"))
            {
                foreach (string containerAttribute in containerAttributes)
                {
                    if (element.HasAttribute(containerAttribute))
                    {
                        parserContext.ReaderContext.ReportException(element, containerAttribute,
                                                                    "The '" + containerAttribute +
                                                                    "' attribute should not be provided when specifying a 'container' reference.");
                    }
                }
                return element.GetAttribute("container");
            }
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(SimpleMessageListenerContainer));
            string destinationAttribute = this.expectReply ? "request-destination" : "destination";
            string destinationNameAttribute = this.expectReply ? "request-destination-name" : "destination-name";
            string destination = element.GetAttribute(destinationAttribute);
            string destinationName = element.GetAttribute(destinationNameAttribute);
            if (!(StringUtils.HasText(destination) ^ StringUtils.HasText(destinationName)))
            {
                parserContext.ReaderContext.ReportException(element, "destination or destination-name",
                        "Exactly one of '" + destinationAttribute + "' or '" + destinationNameAttribute + "' is required.");
            }

            builder.AddPropertyReference(NmsAdapterParserUtils.CONNECTION_FACTORY_PROPERTY,
                        NmsAdapterParserUtils.DetermineConnectionFactoryBeanName(element, parserContext));

            if (StringUtils.HasText(destination))
            {
                builder.AddPropertyReference("destination", destination);
            }
            else
            {
                builder.AddPropertyValue("destinationName", destinationName);
            }
            AcknowledgementMode acknowledgementMode = NmsAdapterParserUtils.ParseAcknowledgementMode(element, parserContext);
            if (acknowledgementMode.Equals(AcknowledgementMode.Transactional))
            {
                builder.AddPropertyValue("sessionTransacted", true);
            }
            else
            {
                builder.AddPropertyValue("sessionAcknowledgeMode", acknowledgementMode);
            }

            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "destination-resolver");
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "transaction-manager");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "pub-sub-domain");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "concurrent-consumers");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "max-concurrent-consumers");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "max-messages-per-task");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "idle-task-execution-limit");
            builder.AddPropertyValue("autoStartup", false);
            return parserContext.ReaderContext.RegisterWithGeneratedName(builder.ObjectDefinition);
        }


        private string ParseMessageListener(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ChannelPublishingJmsMessageListener));

            builder.AddPropertyValue("expectReply", this.expectReply);
            if (this.expectReply)
            {
                IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "request-channel");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "request-timeout");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "reply-timeout");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "extract-request-payload");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "extract-reply-payload");
                int defaults = 0;
                if (StringUtils.HasText(element.GetAttribute(DEFAULT_REPLY_DESTINATION_ATTRIB)))
                {
                    defaults++;
                }
                if (StringUtils.HasText(element.GetAttribute(DEFAULT_REPLY_QUEUE_NAME_ATTRIB)))
                {
                    defaults++;
                }
                if (StringUtils.HasText(element.GetAttribute(DEFAULT_REPLY_TOPIC_NAME_ATTRIB)))
                {
                    defaults++;
                }
                if (defaults > 1)
                {
                    parserContext.ReaderContext.ReportException(element, "several possible properites", "At most one of '" + DEFAULT_REPLY_DESTINATION_ATTRIB
                            + "', '" + DEFAULT_REPLY_QUEUE_NAME_ATTRIB + "', or '" + DEFAULT_REPLY_TOPIC_NAME_ATTRIB
                            + "' may be provided.");
                }
                IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, DEFAULT_REPLY_DESTINATION_ATTRIB);
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, DEFAULT_REPLY_QUEUE_NAME_ATTRIB);
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, DEFAULT_REPLY_TOPIC_NAME_ATTRIB);
                IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "destination-resolver");
            }
            else
            {
                IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "channel", "requestChannel");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "send-timeout", "requestTimeout");
                IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "extract-payload", "extractRequestPayload");
            }
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "message-converter");
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "header-mapper");
            return parserContext.ReaderContext.RegisterWithGeneratedName(builder.ObjectDefinition);
        }
    }
}