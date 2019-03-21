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
using Spring.Integration.Config.Xml;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Adapter.Config {
    /// <summary>
    /// Base class for url-based remoting outbound gateway parsers. 
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractRemotingOutboundGatewayParser : AbstractConsumerEndpointParser {

        protected abstract string GetGatewayClassName(XmlElement element);

        protected override string InputChannelAttributeName {
            get { return "request-channel"; }
        }

        protected override string ResolveId(XmlElement element, AbstractObjectDefinition definition, ParserContext parserContext) {
            string id = base.ResolveId(element, definition, parserContext);
            if(!StringUtils.HasText(id)) {
                id = element.GetAttribute("name");
            }
            if(!StringUtils.HasText(id)) {
                id = parserContext.ReaderContext.GenerateObjectName(definition);
            }
            return id;
        }

        protected override ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(GetGatewayClassName(element));
            string url = ParseUrl(element, parserContext);
            builder.AddConstructorArg(url);
            string replyChannel = element.GetAttribute("reply-channel");
            if(StringUtils.HasText(replyChannel)) {
                builder.AddPropertyReference("replyChannel", replyChannel);
            }
            PostProcessGateway(builder, element, parserContext);
            return builder;
        }

        protected static string ParseUrl(XmlElement element, ParserContext parserContext) {
            string url = element.GetAttribute("url");
            if(!StringUtils.HasText(url)) {
                parserContext.ReaderContext.ReportException(element, "url", "The 'url' attribute is required.");
            }
            return url;
        }

        /**
         * Subclasses may override this method for additional configuration.
         */
        protected virtual void PostProcessGateway(ObjectDefinitionBuilder builder, XmlElement element, ParserContext parserContext) {
        }
    }
}
