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
using Spring.Core;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Shared utility methods for integration namespace parsers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class IntegrationNamespaceUtils {

        public const string BASE_PACKAGE = "Spring.Integration";
        public const string CHANNEL_PACKAGE = BASE_PACKAGE + ".Channel";
        public const string CHANNEL_INTERCEPTOR_PACKAGE = CHANNEL_PACKAGE + ".Interceptor";
        public const string HANDLER_PACKAGE = BASE_PACKAGE + ".Handler";
        public const string CONFIG_PACKAGE = BASE_PACKAGE + ".Config";
        public const string CONFIG_XML_PACKAGE = CONFIG_PACKAGE + ".Xml";
        public const string CONFIG_ANNOTATION_PACKAGE = CONFIG_PACKAGE + ".Annotation";
        public const string FILTER_PACKAGE = BASE_PACKAGE + ".Filter";
        public const string MESSAGE_PACKAGE = BASE_PACKAGE + ".Message";
        public const string SELECTOR_PACKAGE = BASE_PACKAGE + ".Selector";
        public const string ENDPOINT_PACKAGE = BASE_PACKAGE + ".Endpoint";
        public const string INTERNAL_PACKAGE = BASE_PACKAGE + ".Internal";
        public const string SCHEDULING_PACKAGE = BASE_PACKAGE + ".Scheduling";
        public const string AGGREGATOR_PACKAGE = BASE_PACKAGE + ".Aggregator";
        public const string TRANSFORMER_PACKAGE = BASE_PACKAGE + ".Transformer";
        public const string GATEWAY_PACKAGE = BASE_PACKAGE + ".Gateway";
        public const string EVENT_PACKAGE = BASE_PACKAGE + ".Event";

        /// <summary>
        /// Populates the specified object definition property with the value
        /// of the attribute whose name is provided if that attribute is
        /// defined in the given element.
        /// </summary>
        /// <param name="builder">the builder to add the prperty to</param>
        /// <param name="element">the XML element where the attribute should be defined</param>
        /// <param name="attributeName">the name of the attribute whose value will be used to populate the property</param>
        /// <param name="propertyName">the name of the property to be populated</param>
        public static void SetValueIfAttributeDefined(ObjectDefinitionBuilder builder, XmlElement element, string attributeName, string propertyName) {
            string attributeValue = element.GetAttribute(attributeName);
            if(StringUtils.HasText(attributeValue)) {
                builder.AddPropertyValue(propertyName, attributeValue);
            }
        }

        /// <summary>
        /// Populates the object definition property corresponding to the specified
        /// attributeName with the value of that attribute if it is defined in the
        /// given element.
        /// 
        /// <p>The property name will be the camel-case equivalent of the lower
        /// case hyphen separated attribute (e.g. the "foo-bar" attribute would
        /// match the "fooBar" property).
        /// <see cref="Conventions.AttributeNameToPropertyName"/>
        /// </summary>
        /// <param name="builder">the builder</param>
        /// <param name="element">the XML element where the attribute should be defined</param>
        /// <param name="attributeName">the name of the attribute whose value will be set</param>
        public static void SetValueIfAttributeDefined(ObjectDefinitionBuilder builder, XmlElement element, string attributeName) {
            SetValueIfAttributeDefined(builder, element, attributeName, Conventions.AttributeNameToPropertyName(attributeName));
        }

        /// <summary>
        /// Populates the specified object definition property with the reference
        /// to an object. The object reference is identified by the value from the
        /// attribute whose name is provided if that attribute is defined in
        /// the given element.
        /// </summary>
        /// <param name="builder">the builder</param>
        /// <param name="element">the XML element where the attribute should be defined</param>
        /// <param name="attributeName">the name of the attribute whose value will be used as a object reference to populate the property</param>
        /// <param name="propertyName">the name of the property to be populated</param>
        public static void SetReferenceIfAttributeDefined(ObjectDefinitionBuilder builder, XmlElement element, string attributeName, string propertyName) {
            string attributeValue = element.GetAttribute(attributeName);
            if(StringUtils.HasText(attributeValue)) {
                builder.AddPropertyReference(propertyName, attributeValue);
            }
        }

        /// <summary>
        /// Populates the object definition property corresponding to the specified
        /// attributeName with the reference to a object identified by the value of
        /// that attribute if the attribute is defined in the given element.
        /// 
        /// <p>The property name will be the camel-case equivalent of the lower
        /// case hyphen separated attribute (e.g. the "foo-bar" attribute would
        /// match the "fooBar" property).
        /// 
        /// <see cref="Conventions.AttributeNameToPropertyName"/>
        /// </summary>
        /// <param name="builder">the builder</param>
        /// <param name="element">the XML element where the attribute should be defined</param>
        /// <param name="attributeName">the name of the attribute whose value will be used as a object reference to populate the property</param>
        public static void SetReferenceIfAttributeDefined(ObjectDefinitionBuilder builder, XmlElement element, string attributeName) {
            SetReferenceIfAttributeDefined(builder, element, attributeName,
                    Conventions.AttributeNameToPropertyName(attributeName));
        }

        /// <summary>
        /// Parse a "poller" element to provide a reference for the target
        /// ObjectDefinitionBuilder. If the poller element does not contain a "ref"
        /// attribute, this will create and register a PollerMetadata instance and
        /// then add it as a property reference of the target builder.
        /// </summary>
        /// <param name="pollerElement">the "poller" element to parse</param>
        /// <param name="targetBuilder">the builder that expects the "trigger" property</param>
        /// <param name="parserContext">the parserContext for the target builder</param>
        public static void ConfigurePollerMetadata(XmlElement pollerElement, ObjectDefinitionBuilder targetBuilder, ParserContext parserContext) {
            string pollerMetadataRef;
            if(pollerElement.HasAttribute("ref")) {
                if(pollerElement.Attributes.Count != 1) {
                    parserContext.ReaderContext.ReportException(pollerElement, "poller", "A 'poller' element that provides a 'ref' must have no other attributes.");
                }
                if(pollerElement.ChildNodes.Count != 0) {
                    parserContext.ReaderContext.ReportException(pollerElement, "poller", "A 'poller' element that provides a 'ref' must have no child elements.");
                }
                pollerMetadataRef = pollerElement.GetAttribute("ref");
            }
            else {
                IObjectDefinition pollerDef = parserContext.ParserHelper.ParseCustomElement(pollerElement, targetBuilder.ObjectDefinition);
                pollerMetadataRef = parserContext.ReaderContext.RegisterWithGeneratedName(pollerDef);
            }
            targetBuilder.AddPropertyReference("pollerMetadata", pollerMetadataRef);
        }
    }
}