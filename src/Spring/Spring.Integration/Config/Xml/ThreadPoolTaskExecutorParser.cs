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
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading.Execution;
using Spring.Threading.Execution.ExecutionPolicy;
using Spring.Util;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// Parser for the 'thread-pool-task-executor' element.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class ThreadPoolTaskExecutorParser : AbstractSimpleObjectDefinitionParser {

        private const string REJECTION_POLICY_ATTRIBUTE = "rejection-policy";
        private const string CORE_SIZE_ATTRIBUTE = "core-size";
        private const string MAX_SIZE_ATTRIBUTE = "max-size";


        protected override string GetObjectTypeName(XmlElement element) {
            return "Spring.Integration.Util.ThreadPoolTaskExecutor";
        }

        protected override void DoParse(XmlElement element, ObjectDefinitionBuilder builder) {
            foreach(XmlAttribute attribute in element.Attributes) {
                if(IsEligibleAttribute(attribute.Name)) {
                    String propertyName = Conventions.AttributeNameToPropertyName(attribute.LocalName);
                    AssertUtils.State(StringUtils.HasText(propertyName), "Illegal property name returned from 'extractPropertyName(String)': cannot be null or empty.");
                    builder.AddPropertyValue(propertyName, attribute.Value);
                }
            }
            PostProcess(builder, element);
        }

        protected static /*override*/ bool IsEligibleAttribute(string attributeName) {
            return !REJECTION_POLICY_ATTRIBUTE.Equals(attributeName)
                    && !CORE_SIZE_ATTRIBUTE.Equals(attributeName)
                    && !MAX_SIZE_ATTRIBUTE.Equals(attributeName)
                    && !ID_ATTRIBUTE.Equals(attributeName);
            //&& super.isEligibleAttribute(attributeName);
        }

        protected static /*override*/ void PostProcess(ObjectDefinitionBuilder builder, XmlElement element) {
            string policyName = element.GetAttribute(REJECTION_POLICY_ATTRIBUTE);
            builder.AddPropertyValue("rejectedExecutionHandler", CreateRejectedExecutionHandler(policyName));
            builder.AddPropertyValue("corePoolSize", element.GetAttribute(CORE_SIZE_ATTRIBUTE));
            string maxSize = element.GetAttribute(MAX_SIZE_ATTRIBUTE);
            if(StringUtils.HasText(maxSize)) {
                builder.AddPropertyValue("maxPoolSize", maxSize);
            }
        }

        private static IRejectedExecutionHandler CreateRejectedExecutionHandler(IEquatable<string> policyName) {
            if(policyName.Equals("ABORT")) {
                return new AbortPolicy();
            }
            if(policyName.Equals("DISCARD")) {
                return new DiscardPolicy();
            }
            if(policyName.Equals("DISCARD_OLDEST")) {
                return new DiscardOldestPolicy();
            }
            return new CallerRunsPolicy();
        }
    }
}
