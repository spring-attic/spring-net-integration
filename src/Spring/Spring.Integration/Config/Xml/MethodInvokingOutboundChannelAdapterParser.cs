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
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;outbound-channel-adapter/&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MethodInvokingOutboundChannelAdapterParser : AbstractOutboundChannelAdapterParser {

        protected override string ParseAndRegisterConsumer(XmlElement element, ParserContext parserContext) {
            string consumerRef = element.GetAttribute("ref");
            if(!StringUtils.HasText(consumerRef)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "The 'ref' attribute is required.");
            }
            if(element.HasAttribute("method")) {
                consumerRef = parserContext.ReaderContext.RegisterWithGeneratedName(ParseConsumer(element, parserContext));
            }
            return consumerRef;
        }

        protected override AbstractObjectDefinition ParseConsumer(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder invokerBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.HANDLER_PACKAGE + ".MethodInvokingMessageHandler");
            invokerBuilder.AddConstructorArgReference(element.GetAttribute("ref"));
            invokerBuilder.AddConstructorArg(element.GetAttribute("method"));
            string order = element.GetAttribute("order");
            if (StringUtils.HasText(order))
            {
                invokerBuilder.AddPropertyValue("order", order);
            }
            return invokerBuilder.ObjectDefinition;
        }
    }
}
