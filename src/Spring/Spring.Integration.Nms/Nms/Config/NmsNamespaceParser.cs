#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using System;
using System.Xml;
using Spring.Integration.Config.Xml;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Integration.Nms.Config
{
    /// <summary>
    ///  
    /// </summary>
    /// <author>Mark Pollack</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/integration/nms",
            SchemaLocationAssemblyHint = typeof (NmsNamespaceParser),
            SchemaLocation = "/Spring.Integration.Nms.Config/spring-integration-nms-1.0.xsd"
            )
    ]
    public class NmsNamespaceParser : AbstractIntegrationNamespaceParser
    {


        /*
        private readonly NamespaceParserSupport _delegate = new NamespaceParserDelegate();
        private volatile bool _initialized;

        private readonly object _initializationMonitor = new object();

        #region Implementation of INamespaceParser

        public void Init()
        {
            _delegate.Init();
        }

        public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            return _delegate.ParseElement(element, parserContext);
        }

        public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                                               ParserContext parserContext)
        {
            return _delegate.Decorate(node, definition, parserContext);
        }

        #endregion

        private class NamespaceParserDelegate : NamespaceParserSupport
        {
            public override void Init()
            {
                RegisterObjectDefinitionParser("inbound-gateway", new NmsMessageDrivenEndpointParser(true));
                RegisterObjectDefinitionParser("message-driven-channel-adapter",
                                               new NmsMessageDrivenEndpointParser(false));
                RegisterObjectDefinitionParser("inbound-channel-adapter", new NmsInboundChannelAdapterParser());
                RegisterObjectDefinitionParser("outbound-gateway", new NmsOutboundGatewayParser());
                RegisterObjectDefinitionParser("outbound-channel-adapter", new NmsOutboundChannelAdapterParser());
                RegisterObjectDefinitionParser("header-enricher",
                                               new SimpleHeaderEnricherParser(NmsHeaders.PREFIX, new[] {"reply-to"}));
            }
        }
        */

        #region Overrides of AbstractIntegrationNamespaceParser

        public override void Init()
        {
            RegisterObjectDefinitionParser("inbound-gateway", new NmsMessageDrivenEndpointParser(true));
            RegisterObjectDefinitionParser("message-driven-channel-adapter",
                                           new NmsMessageDrivenEndpointParser(false));
            RegisterObjectDefinitionParser("inbound-channel-adapter", new NmsInboundChannelAdapterParser());
            RegisterObjectDefinitionParser("outbound-gateway", new NmsOutboundGatewayParser());
            RegisterObjectDefinitionParser("outbound-channel-adapter", new NmsOutboundChannelAdapterParser());
            RegisterObjectDefinitionParser("header-enricher",
                                           new SimpleHeaderEnricherParser(NmsHeaders.PREFIX, new[] { "reply-to" }));
        }

        #endregion
    }
}