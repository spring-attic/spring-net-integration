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

using System.Xml;
using Spring.Integration.Selector;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;selector-chain/&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class SelectorChainParser : AbstractSingleObjectDefinitionParser {

        protected override string GetObjectTypeName(XmlElement element) {
            return IntegrationNamespaceUtils.SELECTOR_PACKAGE + ".MessageSelectorChain";
        }

        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            if(!StringUtils.HasText(element.GetAttribute("id"))) {
                parserContext.ReaderContext.ReportException(element, element.Name, "id is required");
            }
            ParseSelectorChain(builder, element, parserContext);
        }

        private void ParseSelectorChain(ObjectDefinitionBuilder builder, XmlElement element, ParserContext parserContext) {
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "voting-strategy");
            ManagedList selectors = new ManagedList();
            selectors.ElementTypeName = typeof (IMessageSelector).FullName;
            XmlNodeList childNodes = element.ChildNodes;
            for(int i = 0; i < childNodes.Count; i++) {
                XmlNode child = childNodes.Item(i);
                if(child.NodeType == XmlNodeType.Element) {
                    string nodeName = child.LocalName;
                    if("selector".Equals(nodeName)) {
                        string refatr = ((XmlElement)child).GetAttribute("ref");
                        selectors.Add(new RuntimeObjectReference(refatr));
                    }
                    else if("selector-chain".Equals(nodeName)) {
                        ObjectDefinitionBuilder nestedBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(GetObjectTypeName(null));
                        ParseSelectorChain(nestedBuilder, (XmlElement)child, parserContext);
                        string nestedBeanName = parserContext.ReaderContext.RegisterWithGeneratedName(nestedBuilder.ObjectDefinition);
                        selectors.Add(new RuntimeObjectReference(nestedBeanName));
                    }
                }
            }
            builder.AddPropertyValue("selectors", selectors);
        }
    }
}
