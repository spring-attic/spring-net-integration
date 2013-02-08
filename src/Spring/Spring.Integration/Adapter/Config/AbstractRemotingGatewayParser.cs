#region License

/*
 * Copyright 2002-2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Xml;
using Spring.Core;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Adapter.Config {
    /// <summary>
    /// Base class for remoting gateway parsers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class AbstractRemotingGatewayParser : AbstractSimpleObjectDefinitionParser {

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
        
        protected override void DoParse(XmlElement element, ObjectDefinitionBuilder builder) {
            foreach(XmlAttribute attribute in element.Attributes) {
                if(IsEligibleAttribute(attribute.Name)) {
                    string propertyName = Conventions.AttributeNameToPropertyName(attribute.LocalName);
                    AssertUtils.State(StringUtils.HasText(propertyName), "Illegal property name returned from 'extractPropertyName(String)': cannot be null or empty.");
                    builder.AddPropertyValue(propertyName, attribute.Value);
                }
            }
            PostProcess(builder, element);
        }

        protected static /*override*/ bool IsEligibleAttribute(string attributeName) {
            return !attributeName.Equals("name") && !attributeName.Equals("request-channel")
                   && !attributeName.Equals("reply-channel") && !"id".Equals(attributeName);
        }

        protected /*override*/ void PostProcess(ObjectDefinitionBuilder builder, XmlElement element) {
            string requestChannelRef = element.GetAttribute("request-channel");
            if(!StringUtils.HasText(requestChannelRef))
                throw new ArgumentException("a 'request-channel' reference is required");
            builder.AddPropertyReference("requestChannel", requestChannelRef);
            string replyChannel = element.GetAttribute("reply-channel");
            if(StringUtils.HasText(replyChannel)) {
                builder.AddPropertyReference("replyChannel", replyChannel);
            }
            DoPostProcess(builder, element);
        }

        /**
         * Subclasses may add to the bean definition by overriding this method.
         */
        protected void DoPostProcess(ObjectDefinitionBuilder builder, XmlElement element) {
        }
    }
}