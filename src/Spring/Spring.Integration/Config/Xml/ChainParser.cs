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
using Spring.Integration.Message;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;chain&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class ChainParser : AbstractConsumerEndpointParser {

        protected override ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.HANDLER_PACKAGE + ".MessageHandlerChain");
            ManagedList handlerList = new ManagedList();
            handlerList.ElementTypeName = typeof (IMessageHandler).FullName;
            XmlNodeList children = element.ChildNodes;
            for(int i = 0; i < children.Count; i++) {
                XmlNode child = children.Item(i);
                if(child.NodeType == XmlNodeType.Element && !"poller".Equals(child.LocalName)) {
                    string childBeanName = ParseChild((XmlElement)child, parserContext, builder.ObjectDefinition);
                    handlerList.Add(new RuntimeObjectReference(childBeanName));
                }
            }
            builder.AddPropertyValue("handlers", handlerList);
            return builder;
        }

        private static string ParseChild(XmlElement element, ParserContext parserContext, IObjectDefinition parentDefinition) {
            IObjectDefinition def = parserContext.ParserHelper.ParseCustomElement(element, parentDefinition);
            if(def == null) {
                parserContext.ReaderContext.ReportException(element, element.Name, "child ObjectDefinition must not be null");
            }
            return parserContext.ReaderContext.RegisterWithGeneratedName(def);
        }
    }
}
