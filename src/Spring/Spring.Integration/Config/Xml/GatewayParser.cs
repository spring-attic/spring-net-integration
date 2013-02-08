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
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;gateway/&gt; element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class GatewayParser : AbstractSimpleObjectDefinitionParser {

        private static readonly string[] _referenceAttributes = new[] { "default-request-channel", "default-reply-channel", "message-mapper" };


        protected override string GetObjectTypeName(XmlElement element) {
            return IntegrationNamespaceUtils.GATEWAY_PACKAGE + ".GatewayProxyFactoryObject";
        }

        protected override void DoParse(XmlElement element, ObjectDefinitionBuilder builder) {
            foreach(XmlAttribute attribute in element.Attributes) {
                if(IsEligibleAttribute(attribute.Name)) {
                    string propertyName = Conventions.AttributeNameToPropertyName(attribute.LocalName);
                    AssertUtils.State(StringUtils.HasText(propertyName), "Illegal property name returned from 'extractPropertyName(String)': cannot be null or empty.");
                    builder.AddPropertyValue(propertyName, attribute.Value);
                }
            }

            PostProcess(builder, element);
        }

        protected bool IsEligibleAttribute(string attributeName) {
            if("id".Equals(attributeName))
                return false;

            foreach(string refAttr in _referenceAttributes) {
                if (refAttr.Equals(attributeName))
                    return false;
            }
            return true;            
        }

        protected static void PostProcess(ObjectDefinitionBuilder builder, XmlElement element) {
            foreach(string attributeName in _referenceAttributes) {
                IntegrationNamespaceUtils.SetReferenceIfAttributeDefined(builder, element, attributeName);
            }
        }
    }
}
