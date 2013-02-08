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
using Spring.Integration.Core;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the &lt;header-enricher&gt; element within the core integration
    /// namespace. This is used for setting the <em>standard</em>, out-of-the-box
    /// configurable {@link MessageHeaders}, such as 'reply-channel', 'priority',
    /// and 'correlation-id'. It will also accept custom header values (or bean
    /// references) if provided as 'header' sub-elements.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class StandardHeaderEnricherParser : SimpleHeaderEnricherParser {

        private static readonly string[] REFERENCE_ATTRIBUTES = new[] { "reply-channel", "error-channel" };

        /// <summary>
        /// create a <see cref="StandardHeaderEnricherParser"/>
        /// </summary>
        public StandardHeaderEnricherParser()
            : base(MessageHeaders.PREFIX, REFERENCE_ATTRIBUTES) { }

        protected override void PostProcessHeaders(XmlElement element, ManagedDictionary headers, ParserContext parserContext) {
            XmlNodeList childNodes = element.ChildNodes;
            for(int i = 0; i < childNodes.Count; i++) {
                XmlNode node = childNodes.Item(i);
                if(node.NodeType == XmlNodeType.Element && node.LocalName.Equals("header")) {
                    XmlElement headerElement = (XmlElement)node;
                    string name = headerElement.GetAttribute("name");
                    string value = headerElement.GetAttribute("value");
                    string refatr = headerElement.GetAttribute("ref");
                    bool isValue = StringUtils.HasText(value);
                    bool isRef = StringUtils.HasText(refatr);
                    if(!(isValue ^ isRef)) {
                        parserContext.ReaderContext.ReportException(headerElement, headerElement.Name, "Exactly one of the 'value' or 'ref' attributes is required.");
                    }
                    if(isValue) {
                        headers.Add(name, value);
                    }
                    else {
                        headers.Add(name, new RuntimeObjectReference(refatr));
                    }
                }
            }
        }
    }
}
