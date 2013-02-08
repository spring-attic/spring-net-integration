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

#region

using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Integration.Config.Xml
{
    /// <summary>
    /// Namespace handler for the integration namespace.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/integration",
            SchemaLocationAssemblyHint = typeof (IntegrationNamespaceParser),
            SchemaLocation = "/Spring.Integration.Config.Xml/spring-integration-1.0.xsd"
            )
    ]
    public class IntegrationNamespaceParser : AbstractIntegrationNamespaceParser
    {
        public override void Init()
        {
            RegisterObjectDefinitionParser("channel", new PointToPointChannelParser());
            RegisterObjectDefinitionParser("thread-local-channel", new ThreadLocalChannelParser());
            RegisterObjectDefinitionParser("publish-subscribe-channel", new PublishSubscribeChannelParser());
            RegisterObjectDefinitionParser("service-activator", new ServiceActivatorParser());
            RegisterObjectDefinitionParser("transformer", new TransformerParser());
            RegisterObjectDefinitionParser("filter", new FilterParser());
            RegisterObjectDefinitionParser("router", new RouterParser());
            RegisterObjectDefinitionParser("splitter", new SplitterParser());
            RegisterObjectDefinitionParser("aggregator", new AggregatorParser());
            RegisterObjectDefinitionParser("resequencer", new ResequencerParser());
            RegisterObjectDefinitionParser("header-enricher", new StandardHeaderEnricherParser());
            RegisterObjectDefinitionParser("object-to-string-transformer", new ObjectToStringTransformerParser());
            RegisterObjectDefinitionParser("payload-serializing-transformer", new PayloadSerializingTransformerParser());
            RegisterObjectDefinitionParser("payload-deserializing-transformer", new PayloadDeserializingTransformerParser());
            RegisterObjectDefinitionParser("inbound-channel-adapter", new MethodInvokingInboundChannelAdapterParser());
            RegisterObjectDefinitionParser("outbound-channel-adapter", new MethodInvokingOutboundChannelAdapterParser());
            RegisterObjectDefinitionParser("logging-channel-adapter", new LoggingChannelAdapterParser());
            RegisterObjectDefinitionParser("gateway", new GatewayParser());
            RegisterObjectDefinitionParser("bridge", new BridgeParser());
            RegisterObjectDefinitionParser("chain", new ChainParser());
            RegisterObjectDefinitionParser("selector-chain", new SelectorChainParser());
            RegisterObjectDefinitionParser("poller", new PollerParser());
            //registerObjectDefinitionParser("annotation-config", new AnnotationConfigParser());
            RegisterObjectDefinitionParser("application-event-multicaster", new ApplicationEventMulticasterParser());
            RegisterObjectDefinitionParser("thread-pool-task-executor", new ThreadPoolTaskExecutorParser());
        }
    }
}