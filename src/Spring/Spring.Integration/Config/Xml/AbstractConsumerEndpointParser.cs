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
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Base class parser for elements that create Message Endpoints.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractConsumerEndpointParser : AbstractObjectDefinitionParser {

        protected static string RefAttribute = "ref";

        protected static string MethodAttribute = "method";


        protected override bool ShouldGenerateId {
            get { return false; }
        }

        protected override bool ShouldGenerateIdAsFallback {
            get { return true; }
        }

        /**
         * Parse the MessageHandler.
         */
        protected abstract ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext);

        protected virtual string InputChannelAttributeName {
            get { return "input-channel"; }
        }

        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder handlerBuilder = ParseHandler(element, parserContext);
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(handlerBuilder, element, "output-channel");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(handlerBuilder, element, "order");
            AbstractObjectDefinition handlerBeanDefinition = handlerBuilder.ObjectDefinition;
            string inputChannelAttributeName = InputChannelAttributeName;
            if(!element.HasAttribute(inputChannelAttributeName)) {
                if(!parserContext.IsNested) {
                    parserContext.ReaderContext.ReportException(element, element.Name, "The '" + inputChannelAttributeName
                            + "' attribute is required for top-level endpoint elements.");
                }
                return handlerBeanDefinition;
            }
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CONFIG_PACKAGE + ".ConsumerEndpointFactoryObject");
            string handlerBeanName = parserContext.ReaderContext.RegisterWithGeneratedName(handlerBeanDefinition);
            builder.AddConstructorArgReference(handlerBeanName);
            string inputChannelName = element.GetAttribute(inputChannelAttributeName);
            if(!parserContext.Registry.ContainsObjectDefinition(inputChannelName)) {
                ObjectDefinitionBuilder channelDef = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".DirectChannel");
                ObjectDefinitionHolder holder = new ObjectDefinitionHolder(channelDef.ObjectDefinition, inputChannelName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.Registry);
            }
            builder.AddPropertyValue("inputChannelName", inputChannelName);
            XmlElement pollerElement = DomUtils.GetChildElementByTagName(element, "poller");
            if(pollerElement != null) {
                IntegrationNamespaceUtils.ConfigurePollerMetadata(pollerElement, builder, parserContext);
            }
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "auto-startup");
            return builder.ObjectDefinition;
        }
    }
}
