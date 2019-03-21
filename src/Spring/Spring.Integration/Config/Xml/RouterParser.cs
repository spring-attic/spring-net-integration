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
    /// Parser for the &lt;router/&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class RouterParser : AbstractConsumerEndpointParser {

        protected override ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext) {
            string refAttribute = element.GetAttribute(RefAttribute);
            if(!StringUtils.HasText(refAttribute)) {
                parserContext.ReaderContext.ReportException(element, "router", "The '" + RefAttribute + "' attribute is required.");
            }
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CONFIG_PACKAGE + ".RouterFactoryObject");
            builder.AddPropertyReference("targetObject", refAttribute);
            if(StringUtils.HasText(element.GetAttribute(MethodAttribute))) {
                string method = element.GetAttribute(MethodAttribute);
                builder.AddPropertyValue("targetMethodName", method);
            }
            ObjectDefinitionBuilder resolverBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".ObjectFactoryChannelResolver");
            string resolverObjectName = parserContext.ReaderContext.RegisterWithGeneratedName(resolverBuilder.ObjectDefinition);

            builder.AddPropertyReference("channelResolver", resolverObjectName);
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "default-output-channel");
            return builder;
        }
    }
}
