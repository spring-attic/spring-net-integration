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
    /// Parser for the <em>aggregator</em> element of the integration namespace.
    /// Registers the annotation-driven post-processors.
    /// </summary>
    /// <author>Marius Bogoevici</author>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class AggregatorParser : AbstractConsumerEndpointParser {

        private const string CompletionStrategyRefAttribute = "completion-strategy";
        private const string CompletionStrategyMethodAttribute = "completion-strategy-method";
        private const string DiscardChannelAttribute = "discard-channel";
        private const string SendTimeoutAttribute = "send-timeout";
        private const string SendPartialResultOnTimeoutAttribute = "send-partial-result-on-timeout";
        private const string ReaperIntervalAttribute = "reaper-interval";
        private const string TrackedCorrelationIdCapacityAttribute = "tracked-correlation-id-capacity";
        private const string TimeoutAttribute = "timeout";
        private const string CompletionStrategyProperty = "completionStrategy";


        protected override ObjectDefinitionBuilder ParseHandler(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.AGGREGATOR_PACKAGE + ".MethodInvokingAggregator");
            string refatr = element.GetAttribute(RefAttribute);
            if(!StringUtils.HasText(refatr)) {
                parserContext.ReaderContext.ReportException(element, element.Name, "The '" + RefAttribute + "' attribute is required.");
            }
            builder.AddConstructorArgReference(refatr);
            if(StringUtils.HasText(element.GetAttribute(MethodAttribute))) {
                string method = element.GetAttribute(MethodAttribute);
                builder.AddConstructorArg(method);
            }
            IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, DiscardChannelAttribute);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, SendTimeoutAttribute);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, SendPartialResultOnTimeoutAttribute);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, ReaperIntervalAttribute);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, TrackedCorrelationIdCapacityAttribute);
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, "auto-startup");
            IntegrationNamespaceUtils.SetValueIfAttributeDefined(builder, element, TimeoutAttribute);
            string completionStrategyRef = element.GetAttribute(CompletionStrategyRefAttribute);
            string completionStrategyMethod = element.GetAttribute(CompletionStrategyMethodAttribute);
            if(StringUtils.HasText(completionStrategyRef)) {
                if(StringUtils.HasText(completionStrategyMethod)) {
                    string adapterBeanName = CreateCompletionStrategyAdapter(
                            completionStrategyRef, completionStrategyMethod, parserContext);
                    builder.AddPropertyReference(CompletionStrategyProperty, adapterBeanName);
                }
                else {
                    builder.AddPropertyReference(CompletionStrategyProperty, completionStrategyRef);
                }
            }
            return builder;
        }

        private static string CreateCompletionStrategyAdapter(string refatr, string method, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.AGGREGATOR_PACKAGE + ".CompletionStrategyAdapter");
            builder.AddConstructorArgReference(refatr);
            builder.AddConstructorArg(method);
            return parserContext.ReaderContext.RegisterWithGeneratedName(builder.ObjectDefinition);
        }
    }
}
