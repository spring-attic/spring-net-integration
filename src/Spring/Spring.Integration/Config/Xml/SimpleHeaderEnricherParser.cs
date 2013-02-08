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
using System.Xml;
using Spring.Core;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;

namespace Spring.Integration.Config.Xml {
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
public class SimpleHeaderEnricherParser : AbstractTransformerParser {

	private readonly static string[] ineligibleHeaderNames = new [] {"id", "input-channel", "output-channel", "overwrite"};


	private readonly string _prefix;

	private readonly string[] _referenceAttributes;


	public SimpleHeaderEnricherParser() 
		: this(null, null)
	{}

	public SimpleHeaderEnricherParser(string prefix) 
        :this(prefix, null)
	{}

	public SimpleHeaderEnricherParser(string prefix, string[] referenceAttributes) {
		_prefix = prefix;
		_referenceAttributes = referenceAttributes != null ? referenceAttributes : new string[0];
	}


	protected override string TransformerClassName {
		get { return IntegrationNamespaceUtils.TRANSFORMER_PACKAGE + ".HeaderEnricher"; }
	}

	protected static bool IsEligibleHeaderName(string headerName) {
		foreach(string s in ineligibleHeaderNames) {
		    if (s.Equals(headerName))
		        return false;
		}

	    return true;
	}

	protected static bool ShouldOverwrite(XmlElement element) {
		return "true".Equals(element.GetAttribute("overwrite").ToLower());
	}

	protected override void ParseTransformer(XmlElement element, ParserContext parserContext, ObjectDefinitionBuilder builder) {
		ManagedDictionary headers = new ManagedDictionary();
		XmlAttributeCollection attributes = element.Attributes;
		for (int i = 0; i < attributes.Count; i++) {
			XmlNode node = attributes.Item(i);
			String name = node.LocalName;
			if (IsEligibleHeaderName(name)) {
				name = Conventions.AttributeNameToPropertyName(name);
                object value;
                if(ReferenceAttributesContains(name))
                    value = new RuntimeObjectReference(node.Value);
                else
                    value = node.Value;
				
                if (_prefix != null) {
					name = _prefix + name;
				}
				headers.Add(name, value);
			}
		}
		PostProcessHeaders(element, headers, parserContext);
		builder.AddConstructorArg(headers);
		builder.AddPropertyValue("overwrite", ShouldOverwrite(element));
	}

    private bool ReferenceAttributesContains(string name) {
        foreach(string s in _referenceAttributes) {
            if(s.Equals(name))
                return true;
        }
        return false;
    }

	/**
	 * Subclasses may implement this method to provide additional headers.
	 */
	protected virtual void PostProcessHeaders(XmlElement element, ManagedDictionary headers, ParserContext parserContext) {
	}
}
}
