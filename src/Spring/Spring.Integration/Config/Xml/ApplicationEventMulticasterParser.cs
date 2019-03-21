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

using System;
using System.Xml;
using Spring.Collections;
using Spring.Context.Support;
using Spring.Integration.Context;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;application-event-multicaster&gt; element of the
    /// integration namespace.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ApplicationEventMulticasterParser : AbstractSingleObjectDefinitionParser {

        protected override String GetObjectTypeName(XmlElement element) {
            return IntegrationNamespaceUtils.EVENT_PACKAGE + ".SimpleApplicationEventMulticaster";
        }

        protected override string ResolveId(XmlElement element, AbstractObjectDefinition definition, ParserContext parserContext) {
            // TODO return AbstractApplicationContext.APPLICATION_EVENT_MULTICASTER_BEAN_NAME;
            return AbstractApplicationContext.EventRegistryObjectName; //.APPLICATION_EVENT_MULTICASTER_BEAN_NAME;
        }

        protected override void DoParse(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
            String taskExecutorRef = element.GetAttribute("task-executor");
            if(StringUtils.HasText(taskExecutorRef)) {
                builder.AddPropertyReference("taskExecutor", taskExecutorRef);
            }
            else {
                IExecutor taskExecutor = IntegrationContextUtils.CreateThreadPoolTaskExecutor(1, 10, 0, "event-multicaster-");
                builder.AddPropertyValue("taskExecutor", taskExecutor);
            }
            // TODO CopyOnWriteArraySet
            builder.AddPropertyValue("collectionClass", typeof (Set)); //CopyOnWriteArraySet));
        }
    }
}
