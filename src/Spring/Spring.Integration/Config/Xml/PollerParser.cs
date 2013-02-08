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
using System.Collections;
using System.Data;
using System.Xml;
using AopAlliance.Aop;
using Spring.Core.TypeConversion;
using Spring.Integration.Context;
using Spring.Integration.Utils;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Transaction;
using Spring.Transaction.Support;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;poller&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class PollerParser : AbstractObjectDefinitionParser {

        protected override string ResolveId(XmlElement element, AbstractObjectDefinition definition, ParserContext parserContext) {
            string id = base.ResolveId(element, definition, parserContext);
            if(element.GetAttribute("default").Equals("true")) {
                if(parserContext.Registry.GetObjectDefinition(IntegrationContextUtils.DefaultPollerMetadataObjectName) != null) {
                    parserContext.ReaderContext.ReportException(element, element.Name, "Only one default <poller/> element is allowed per context.");
                }
                if(StringUtils.HasText(id)) {
                    parserContext.Registry.RegisterAlias(id, IntegrationContextUtils.DefaultPollerMetadataObjectName);
                }
                else {
                    id = IntegrationContextUtils.DefaultPollerMetadataObjectName;
                }
            }
            else if(!StringUtils.HasText(id)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "The 'id' attribute is required for a top-level poller element unless it is the default poller.");
            }
            return id;
        }

        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder metadataBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.SCHEDULING_PACKAGE + ".PollerMetadata");
            if(element.HasAttribute("ref")) {
                parserContext.ReaderContext.ReportException(element, element.Name, "the 'ref' attribute must not be present on a 'poller' element submitted to the parser");
            }
            ConfigureTrigger(element, metadataBuilder, parserContext);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(metadataBuilder, element, "max-messages-per-poll");
            XmlElement adviceChainElement = DomUtils.GetChildElementByTagName(element, "advice-chain");
            if(adviceChainElement != null) {
                ConfigureAdviceChain(adviceChainElement, metadataBuilder, parserContext);
            }
            XmlElement txElement = DomUtils.GetChildElementByTagName(element, "transactional");
            if(txElement != null) {
                ConfigureTransactionAttributes(txElement, metadataBuilder);
            }
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(metadataBuilder, element, "task-executor");
            return metadataBuilder.ObjectDefinition;
        }

        private static void ConfigureTrigger(XmlElement pollerElement, ObjectDefinitionBuilder targetBuilder, ParserContext parserContext) {
            string triggerBeanName;
            XmlElement intervalElement = DomUtils.GetChildElementByTagName(pollerElement, "interval-trigger");
            if(intervalElement != null) {
                triggerBeanName = ParseIntervalTrigger(intervalElement, parserContext);
            }
            else {
                XmlElement cronElement = DomUtils.GetChildElementByTagName(pollerElement, "cron-trigger");
                if(cronElement == null) {
                    parserContext.ReaderContext.ReportException(pollerElement, pollerElement.Name, "A <poller> element must include either an <interval-trigger/> or <cron-trigger/> child element.");
                }
                triggerBeanName = ParseCronTrigger(cronElement, parserContext);
            }
            targetBuilder.AddPropertyReference("trigger", triggerBeanName);
        }

        /**
         * Parse an "interval-trigger" element
         */
        private static string ParseIntervalTrigger(XmlElement element, ParserContext parserContext) {
            string interval = element.GetAttribute("interval");
            if(!StringUtils.HasText(interval)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "the 'interval' attribute is required for an <interval-trigger/>");
            }
            //TimeUnit timeUnit = TimeUnit.valueOf(element.getAttribute("time-unit"));
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.SCHEDULING_PACKAGE + ".IntervalTrigger");
          
            //TODO MLP handle parsing of time-unit correctly, now hardcode use of 'seconds' via Spring.Core.TypeConversion.TimeSpanConverter                       
            string timeUnit = element.GetAttribute("time-unit");
            switch (timeUnit)
            {
                case "SECONDS":
                    interval = interval + "s";
                    break;
                case "MILLISECONDS":
                    interval = interval + "ms";
                    break;
            }
            builder.AddConstructorArg(interval);
            
             
            //builder.addConstructorArgValue(timeUnit);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "initial-delay");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "fixed-rate");
            return parserContext.ReaderContext.RegisterWithGeneratedName(builder.ObjectDefinition);
        }

        /**
         * Parse a "cron-trigger" element
         */
        private static string ParseCronTrigger(XmlElement element, ParserContext parserContext) {
            string cronExpression = element.GetAttribute("expression");
            if(!StringUtils.HasText(cronExpression)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "the 'expression' attribute is required for a <cron-trigger/>");
            }
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.SCHEDULING_PACKAGE + ".CronTrigger");
            builder.AddConstructorArg(cronExpression);
            return parserContext.ReaderContext.RegisterWithGeneratedName(builder.ObjectDefinition);
        }

        /**
         * Parse a "transactional" element and configure the "transactionManager" and "transactionDefinition"
         * properties for the target builder.
         */
        private static void ConfigureTransactionAttributes(XmlElement txElement, ObjectDefinitionBuilder targetBuilder) {
            targetBuilder.AddPropertyReference("transactionManager", txElement.GetAttribute("transaction-manager"));
            DefaultTransactionDefinition txDefinition = new DefaultTransactionDefinition();
            txDefinition.PropagationBehavior = (TransactionPropagation)Enum.Parse(typeof(TransactionPropagation), txElement.GetAttribute("propagation"));
            txDefinition.TransactionIsolationLevel = (IsolationLevel)Enum.Parse(typeof(IsolationLevel), txElement.GetAttribute("isolation"));
            txDefinition.TransactionTimeout = Convert.ToInt32(txElement.GetAttribute("timeout"));
            txDefinition.ReadOnly = txElement.GetAttribute("read-only").Equals("true", StringComparison.OrdinalIgnoreCase);
            targetBuilder.AddPropertyValue("transactionDefinition", txDefinition);
        }

        /**
         * Parses the 'advice-chain' element's sub-elements.
         */
        private static void ConfigureAdviceChain(XmlNode adviceChainElement, ObjectDefinitionBuilder targetBuilder, ParserContext parserContext) {
            ManagedList adviceChain = new ManagedList();
            adviceChain.ElementTypeName = typeof (IAdvice).FullName;
            XmlNodeList childNodes = adviceChainElement.ChildNodes;
            for(int i = 0; i < childNodes.Count; i++) {
                XmlNode child = childNodes.Item(i);
                if(child.NodeType == XmlNodeType.Element) {
                    XmlElement childElement = (XmlElement)child;
                    string localName = child.LocalName;
                    if("object".Equals(localName)) {
                        //ObjectDefinitionHolder holder = parserContext.ParserHelper.ParseObjectDefinitionElement(childElement, targetBuilder.ObjectDefinition);
                        IObjectDefinition def = parserContext.ParserHelper.ParseCustomElement(childElement, targetBuilder.ObjectDefinition);
                        string name = parserContext.ReaderContext.RegisterWithGeneratedName(def);
                        adviceChain.Add(new RuntimeObjectReference(name));
                    }
                    else if("ref".Equals(localName)) {
                        String refatr = childElement.GetAttribute("object");
                        adviceChain.Add(new RuntimeObjectReference(refatr));
                    }
                    else {
                        IObjectDefinition customBeanDefinition = parserContext.ParserHelper.ParseCustomElement(childElement, targetBuilder.ObjectDefinition);
                        if(customBeanDefinition == null) {
                            parserContext.ReaderContext.ReportException(childElement, childElement.Name, "failed to parse custom element '" + localName + "'");
                        }
                    }
                }
            }
            targetBuilder.AddPropertyValue("adviceChain", adviceChain);
        }
    }
}
