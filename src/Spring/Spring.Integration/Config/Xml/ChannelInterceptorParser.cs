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

using System.Collections.Generic;
using System.Xml;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Integration.Config.Xml 
{

/**
 * A helper class for parsing the sub-elements of a channel's
 * <em>interceptors</em> element.
 * 
 * @author Mark Fisher
 */
public class ChannelInterceptorParser {

	private IDictionary<string, IObjectDefinitionRegisteringParser> parsers;


	public ChannelInterceptorParser() {
		parsers = new Dictionary<string, IObjectDefinitionRegisteringParser>();
		
        parsers["wire-tap"] = new WireTapParser();
	}


    public ManagedList ParseInterceptors(XmlElement element, ParserContext parserContext) {
        ManagedList interceptors = new ManagedList();
		foreach(XmlNode child in element.ChildNodes) {
			
            /* TODO:
             * check full elementname (incl. NamespaceUri)
            */
            if (child.NodeType == XmlNodeType.Element) {
				XmlElement childElement = (XmlElement) child;
				string localName = child.LocalName;
				if ("object".Equals(localName)) {
				    ObjectDefinitionHolder holder = parserContext.ParserHelper.ParseObjectDefinitionElement(childElement);
                    interceptors.Add(holder);
				}
				else if ("ref".Equals(localName)) {
                    string reference = childElement.GetAttribute("object");
                    interceptors.Add(new RuntimeObjectReference(reference));
				}
				else {
					if (!parsers.ContainsKey(localName)) {
					    parserContext.ReaderContext.ReportException(childElement, localName, "unsupported interceptor element");
					}
                    IObjectDefinitionRegisteringParser parser = parsers[localName];
                    string interceptorObjectName = parser.Parse(childElement, parserContext);
                    interceptors.Add(new RuntimeObjectReference(interceptorObjectName));
				}
			}
		}
		return interceptors;
	}

}
}