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
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Base parser for Channel Adapters.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractChannelAdapterParser : AbstractObjectDefinitionParser {

        protected override string ResolveId(XmlElement element, AbstractObjectDefinition definition, ParserContext parserContext) {
            string id = element.GetAttribute("id");
            if(!element.HasAttribute("channel")) {
                // the created channel will get the 'id', so the adapter's object name includes a suffix
                id = id + ".adapter";
            }
            else if(!StringUtils.HasText(id)) {
                id = parserContext.ReaderContext.GenerateObjectName(definition);
            }
            return id;
        }

        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext) {
            string channelName = element.GetAttribute("channel");
            if(!StringUtils.HasText(channelName)) {
                channelName = CreateDirectChannel(element, parserContext);
            }
            return DoParse(element, parserContext, channelName);
        }

        private static string CreateDirectChannel(XmlElement element, ParserContext parserContext) {
            string channelId = element.GetAttribute("id");
            if(!StringUtils.HasText(channelId)) {
                parserContext.ReaderContext.ReportException(element, "channel", "The channel-adapter's 'id' attribute is required when no 'channel' "
                        + "reference has been provided, because that 'id' would be used for the created channel.");
            }
            ObjectDefinitionBuilder channelBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".DirectChannel");
            ObjectDefinitionHolder holder = new ObjectDefinitionHolder(channelBuilder.ObjectDefinition, channelId);
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.Registry);
            return channelId;
        }

        /**
         * Subclasses must implement this method to parse the adapter element.
         * The name of the MessageChannel object is provided.
         */
        protected abstract AbstractObjectDefinition DoParse(XmlElement element, ParserContext parserContext, string channelName);
    }
}
