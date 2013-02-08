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
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;publish-subscribe-channel&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PublishSubscribeChannelParser : AbstractChannelParser {

        protected override ObjectDefinitionBuilder BuildObjectDefinition(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".PublishSubscribeChannel");
            string taskExecutorRef = element.GetAttribute("task-executor");
            if(StringUtils.HasText(taskExecutorRef)) {
                builder.AddConstructorArgReference(taskExecutorRef);
            }
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, "error-handler");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "apply-sequence");
            return builder;
        }
    }
}
