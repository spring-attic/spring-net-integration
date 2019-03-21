#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Xml;
using Spring.Integration.Utils;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Base class for outbound Channel Adapter parsers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractOutboundChannelAdapterParser : AbstractChannelAdapterParser {

        protected override AbstractObjectDefinition DoParse(XmlElement element, ParserContext parserContext, string channelName) {
            XmlElement pollerElement = DomUtils.GetChildElementByTagName(element, "poller");
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CONFIG_PACKAGE + ".ConsumerEndpointFactoryObject");
            builder.AddConstructorArgReference(ParseAndRegisterConsumer(element, parserContext));
            if(pollerElement != null) {
                if(!StringUtils.HasText(channelName)) {
                    parserContext.ReaderContext.ReportException(element, element.Name, "outbound channel adapter with a 'poller' requires a 'channel' to poll");
                }
                IntegrationNamespaceUtils.ConfigurePollerMetadata(pollerElement, builder, parserContext);
            }
            builder.AddPropertyValue("inputChannelName", channelName);
            return builder.ObjectDefinition;
        }

        /// <summary>
        /// Override this method to control the registration process and return the bean name.
        /// If parsing a bean definition whose name can be auto-generated, consider using
        /// {@link #parseConsumer(Element, ParserContext)} instead.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="parserContext"></param>
        /// <returns></returns>
        protected virtual string ParseAndRegisterConsumer(XmlElement element, ParserContext parserContext) {
            AbstractObjectDefinition definition = ParseConsumer(element, parserContext);
            if(definition == null) {
                parserContext.ReaderContext.ReportException(element, element.Name, "Consumer parsing must return a ObjectDefinition.");
            }
            string order = element.GetAttribute("order");
            if (StringUtils.HasText(order) && definition != null)
            {
                definition.PropertyValues.Add("order", order);
            }
            return parserContext.ReaderContext.RegisterWithGeneratedName(definition);
        }

        /// <summary>
        /// Override this method to return the BeanDefinition for the MessageConsumer. It will
        /// be registered with a generated name.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="parserContext"></param>
        /// <returns></returns>
        protected abstract AbstractObjectDefinition ParseConsumer(XmlElement element, ParserContext parserContext);
    }
}