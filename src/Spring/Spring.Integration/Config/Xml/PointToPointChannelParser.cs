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
using Spring.Integration.Utils;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;channel&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PointToPointChannelParser : AbstractChannelParser {

        protected override ObjectDefinitionBuilder BuildObjectDefinition(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder;
            XmlElement queueElement;
            if((queueElement = DomUtils.GetChildElementByTagName(element, "queue")) != null) {
                builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".QueueChannel");
                ParseQueueCapacity(builder, queueElement);
            }
            else if((queueElement = DomUtils.GetChildElementByTagName(element, "priority-queue")) != null) {
                builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".PriorityChannel");
                ParseQueueCapacity(builder, queueElement);
                string comparatorRef = queueElement.GetAttribute("comparator");
                if(StringUtils.HasText(comparatorRef)) {
                    builder.AddConstructorArgReference(comparatorRef);
                }
            }
            else if(DomUtils.GetChildElementByTagName(element, "rendezvous-queue") != null) {
                builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".RendezvousChannel");
            }
            else {
                builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".DirectChannel");
            }
            return builder;
        }

        private static void ParseQueueCapacity(ObjectDefinitionBuilder builder, XmlElement queueElement) {
            string capacity = queueElement.GetAttribute("capacity");
            if (StringUtils.HasText(capacity)) {
                builder.AddConstructorArg(Convert.ToUInt32(capacity));
            }
        }
    }
}