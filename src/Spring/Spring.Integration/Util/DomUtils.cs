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
using Spring.Util;

namespace Spring.Integration.Utils {

    /**
     * Convenience methods for working with the DOM API,
     * in particular for working with DOM Nodes and DOM Elements.
     *
     * @author Juergen Hoeller
     * @author Rob Harrop
     * @author Costin Leau
     * @since 1.2
     * @see org.w3c.dom.Node
     * @see org.w3c.dom.Element
     */
    public abstract class DomUtils {

        ///**
        // * Retrieve all child elements of the given DOM element that match any of
        // * the given element names. Only look at the direct child level of the
        // * given element; do not go into further depth (in contrast to the
        // * DOM API's <code>getElementsByTagName</code> method).
        // * @param ele the DOM element to analyze
        // * @param childEleNames the child element names to look for
        // * @return a List of child <code>org.w3c.dom.Element</code> instances
        // * @see org.w3c.dom.Element
        // * @see org.w3c.dom.Element#getElementsByTagName
        // */
        //public static List getChildElementsByTagName(Element ele, string[] childEleNames) {
        //    Assert.notNull(ele, "Element must not be null");
        //    Assert.notNull(childEleNames, "Element names collection must not be null");
        //    List childEleNameList = Arrays.asList(childEleNames);
        //    NodeList nl = ele.getChildNodes();
        //    List childEles = new ArrayList();
        //    for (int i = 0; i < nl.getLength(); i++) {
        //        Node node = nl.item(i);
        //        if (node instanceof Element && nodeNameMatch(node, childEleNameList)) {
        //            childEles.add(node);
        //        }
        //    }
        //    return childEles;
        //}

        /**
         * Retrieve all child elements of the given DOM element that match
         * the given element name. Only look at the direct child level of the
         * given element; do not go into further depth (in contrast to the
         * DOM API's <code>getElementsByTagName</code> method).
         * @param ele the DOM element to analyze
         * @param childEleName the child element name to look for
         * @return a List of child <code>org.w3c.dom.Element</code> instances
         * @see org.w3c.dom.Element
         * @see org.w3c.dom.Element#getElementsByTagName
         */
        //public static List getChildElementsByTagName(Element ele, string childEleName) {
        //    return getChildElementsByTagName(ele, new string[] { childEleName });
        //}

        /// <summary>
        /// Utility method that returns the first child element
        /// identified by its name.
        /// </summary>
        /// <param name="element">the DOM element to analyze</param>
        /// <param name="childElementName">childEleName the child element name to look for</param>
        /// <returns>the <c>org.w3c.dom.Element</c> instance or <c>null</c> if none found</returns>
        public static XmlElement GetChildElementByTagName(XmlElement element, string childElementName) {
            AssertUtils.ArgumentNotNull(element, "element", "Element must not be null");
            AssertUtils.ArgumentNotNull(childElementName, "childElementName", "child element name must not be null");

            foreach(XmlNode node in element.ChildNodes) {
                if(node is XmlElement && NodeNameMatch(node, childElementName)) {
                    return (XmlElement)node;
                }
            }
            return null;
        }

        ///**
        // * Utility method that returns the first child element value
        // * identified by its name.
        // * @param ele the DOM element to analyze
        // * @param childEleName the child element name to look for
        // * @return the extracted text value,
        // * or <code>null</code> if no child element found
        // */
        //public static string getChildElementValueByTagName(Element ele, string childEleName) {
        //    Element child = getChildElementByTagName(ele, childEleName);
        //    return (child != null ? getTextValue(child) : null);
        //}

        ///**
        // * Extract the text value from the given DOM element, ignoring XML comments.
        // * <p>Appends all CharacterData nodes and EntityReference nodes
        // * into a single string value, excluding Comment nodes.
        // * @see CharacterData
        // * @see EntityReference
        // * @see Comment
        // */
        //public static string getTextValue(Element valueEle) {
        //    Assert.notNull(valueEle, "Element must not be null");
        //    StringBuffer value = new StringBuffer();
        //    NodeList nl = valueEle.getChildNodes();
        //    for (int i = 0; i < nl.getLength(); i++) {
        //        Node item = nl.item(i);
        //        if ((item instanceof CharacterData && !(item instanceof Comment)) || item instanceof EntityReference) {
        //            value.append(item.getNodeValue());
        //        }
        //    }
        //    return value.toString();
        //}

        ///**
        // * Namespace-aware equals comparison. Returns <code>true</code> if either
        // * {@link Node#getLocalName} or {@link Node#getNodeName} equals <code>desiredName</code>,
        // * otherwise returns <code>false</code>.
        // */
        //public static boolean nodeNameEquals(Node node, string desiredName) {
        //    Assert.notNull(node, "Node must not be null");
        //    Assert.notNull(desiredName, "Desired name must not be null");
        //    return nodeNameMatch(node, desiredName);
        //}

        /// <summary>
        /// Matches the given node's name and local name against the given desired name.
        /// </summary>
        private static bool NodeNameMatch(XmlNode node, string desiredName) {
            return (desiredName.Equals(node.Name) || desiredName.Equals(node.LocalName));
        }

        ///**
        // * Matches the given node's name and local name against the given desired names.
        // */
        //private static boolean nodeNameMatch(Node node, Collection desiredNames) {
        //    return (desiredNames.contains(node.getNodeName()) || desiredNames.contains(node.getLocalName()));
        //}

    }
}
