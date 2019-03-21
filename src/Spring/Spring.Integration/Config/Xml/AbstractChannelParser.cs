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

using System.Xml;
using Spring.Integration.Utils;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {

    /// <summary>
    /// Base class for channel parsers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class AbstractChannelParser : AbstractObjectDefinitionParser {

        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext) {
            ObjectDefinitionBuilder builder = BuildObjectDefinition(element, parserContext);
            ManagedList interceptors = null;
            XmlElement interceptorsElement = DomUtils.GetChildElementByTagName(element, "interceptors");

            if(interceptorsElement != null) {
                ChannelInterceptorParser interceptorParser = new ChannelInterceptorParser();
                interceptors = interceptorParser.ParseInterceptors(interceptorsElement, new ParserContext(parserContext.ParserHelper, builder.RawObjectDefinition));
            }
            if(interceptors == null) {
                interceptors = new ManagedList();
            }

            string datatypeAttr = element.GetAttribute("datatype");
            if(StringUtils.HasText(datatypeAttr)) {
                string[] datatypes = StringUtils.CommaDelimitedListToStringArray(datatypeAttr);
                RootObjectDefinition selectorDef = new RootObjectDefinition();
                selectorDef.ObjectTypeName = IntegrationNamespaceUtils.SELECTOR_PACKAGE + ".PayloadTypeSelector";
                selectorDef.ConstructorArgumentValues.AddGenericArgumentValue(datatypes);
                string selectorObjectName = parserContext.ReaderContext.RegisterWithGeneratedName(selectorDef);

                RootObjectDefinition interceptorDef = new RootObjectDefinition();
                interceptorDef.ObjectTypeName = IntegrationNamespaceUtils.CHANNEL_INTERCEPTOR_PACKAGE + ".MessageSelectingInterceptor";
                interceptorDef.ConstructorArgumentValues.AddGenericArgumentValue(new RuntimeObjectReference(selectorObjectName));
                string interceptorObjectName = parserContext.ReaderContext.RegisterWithGeneratedName(interceptorDef);

                interceptors.Add(new RuntimeObjectReference(interceptorObjectName));
            }

            builder.AddPropertyValue("interceptors", interceptors);
            return builder.ObjectDefinition;
        }

        /// <summary>
        /// Subclasses must implement this method to create the bean definition.
        /// The class must be defined, and any implementation-specific constructor
        /// arguments or properties should be configured. This base class will
        /// configure the interceptors including the 'datatype' interceptor if
        /// the 'datatype' attribute is defined on the channel element.
        /// </summary>
        /// <param name="element">the defining xml element</param>
        /// <param name="parserContext">the parser context</param>
        /// <returns></returns>
        protected abstract ObjectDefinitionBuilder BuildObjectDefinition(XmlElement element, ParserContext parserContext);
    }
}
