#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using System.Xml;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Integration.Config.Xml
{
    /// <summary>
    ///  
    /// </summary>
    /// <author>Mark Pollack</author>
    public abstract class AbstractIntegrationNamespaceParser : INamespaceParser
    {
        public const string DEFAULT_CONFIGURING_POSTPROCESSOR_SIMPLE_CLASS_NAME =
            "DefaultConfiguringObjectFactoryPostProcessor";

        public const string DEFAULT_CONFIGURING_POSTPROCESSOR_OBJECT_NAME =
            IntegrationNamespaceUtils.INTERNAL_PACKAGE + "." + DEFAULT_CONFIGURING_POSTPROCESSOR_SIMPLE_CLASS_NAME;

        private IntNamespaceHandlerDelegate namespaceHandlerDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        protected AbstractIntegrationNamespaceParser()
        {
            namespaceHandlerDelegate = new IntNamespaceHandlerDelegate(this);
        }

        #region Implementation of INamespaceParser

        public abstract void Init();

        public IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            RegisterDefaultConfiguringObjectFactoryPostProcessorIfNecessary(parserContext);
            return namespaceHandlerDelegate.ParseElement(element, parserContext);
        }

        public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                                               ParserContext parserContext)
        {
            return namespaceHandlerDelegate.Decorate(node, definition, parserContext);
        }

        #endregion

        private void RegisterDefaultConfiguringObjectFactoryPostProcessorIfNecessary(ParserContext parserContext)
        {
            if (!parserContext.Registry.IsObjectNameInUse(DEFAULT_CONFIGURING_POSTPROCESSOR_OBJECT_NAME))
            {
                ObjectDefinitionBuilder builder =
                    ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CONFIG_XML_PACKAGE + "." +
                                                                    DEFAULT_CONFIGURING_POSTPROCESSOR_SIMPLE_CLASS_NAME);
                ObjectDefinitionHolder holder = new ObjectDefinitionHolder(builder.ObjectDefinition,
                                                                           DEFAULT_CONFIGURING_POSTPROCESSOR_OBJECT_NAME);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.Registry);
            }
        }

        protected void RegisterObjectDefinitionParser(string elementName, IObjectDefinitionParser parser)
        {
            namespaceHandlerDelegate.DoRegisterObjectDefinitionParser(elementName, parser);
        }

        public class IntNamespaceHandlerDelegate : NamespaceParserSupport
        {
            private INamespaceParser outer;

            public IntNamespaceHandlerDelegate(INamespaceParser outer)
            {
                this.outer = outer;
            }

            public override void Init()
            {
                outer.Init();
            }

            public void DoRegisterObjectDefinitionParser(string elementName, IObjectDefinitionParser parser)
            {
                base.RegisterObjectDefinitionParser(elementName, parser);
            }
        }
    }
}