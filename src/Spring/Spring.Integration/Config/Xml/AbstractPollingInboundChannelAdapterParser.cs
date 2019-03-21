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
    /// Base parser for inbound Channel Adapters that poll a source.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractPollingInboundChannelAdapterParser : AbstractChannelAdapterParser {

        protected override AbstractObjectDefinition DoParse(XmlElement element, ParserContext parserContext, string channelName) {
            string source = ParseSource(element, parserContext);
            if(!StringUtils.HasText(source)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "failed to parse source");
            }
            ObjectDefinitionBuilder adapterBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CONFIG_PACKAGE + ".SourcePollingChannelAdapterFactoryObject");
            adapterBuilder.AddPropertyReference("source", source);
            adapterBuilder.AddPropertyReference("outputChannel", channelName);
            XmlElement pollerElement = DomUtils.GetChildElementByTagName(element, "poller");
            if(pollerElement != null) {
                IntegrationNamespaceUtils.ConfigurePollerMetadata(pollerElement, adapterBuilder, parserContext);
            }
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(adapterBuilder, element, "auto-startup");
            return adapterBuilder.ObjectDefinition;
        }

        /**
         * Subclasses must implement this method to parse the PollableSource instance
         * which the created Channel Adapter will poll.
         */
        protected abstract string ParseSource(XmlElement element, ParserContext parserContext);
    }
}
