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

using System.Xml;
using Apache.NMS;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Integration.Nms.Config
{
    /// <summary>
    /// Utility methods and constants for JMS adapter parsers.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    /// <author>Mark Fisher</author>
    public class NmsAdapterParserUtils
    {
        public const string NMS_TEMPLATE_ATTRIBUTE = "nms-template";

        public const string NMS_TEMPLATE_PROPERTY = "nmsTemplate";

        public const string CONNECTION_FACTORY_ATTRIBUTE = "connection-factory";

        public const string CONNECTION_FACTORY_PROPERTY = "connectionFactory";

        public const string DESTINATION_ATTRIBUTE = "destination";

        public const string DESTINATION_PROPERTY = "destination";

        public const string DESTINATION_NAME_ATTRIBUTE = "destination-name";

        public const string DESTINATION_NAME_PROPERTY = "destinationName";

        public const string HEADER_MAPPER_ATTRIBUTE = "header-mapper";

        public const string HEADER_MAPPER_PROPERTY = "headerMapper";

        public const string ACKNOWLEDGE_ATTRIBUTE = "acknowledge";

        public const string ACKNOWLEDGE_AUTO = "auto";

        public const string ACKNOWLEDGE_CLIENT = "client";

        public const string ACKNOWLEDGE_DUPS_OK = "dups-ok";

        public const string ACKNOWLEDGE_TRANSACTED = "transacted";


        public static string DetermineConnectionFactoryBeanName(XmlElement element, ParserContext parserContext)
        {
            string connectionFactoryBeanName = "connectionFactory";
            if (element.HasAttribute(CONNECTION_FACTORY_ATTRIBUTE))
            {
                connectionFactoryBeanName = element.GetAttribute(CONNECTION_FACTORY_ATTRIBUTE);
                if (!StringUtils.HasText(connectionFactoryBeanName))
                {
                    parserContext.ReaderContext.ReportException(element, element.Name, "NMS adapter 'connection-factory' attribute must not be empty");
                }
            }
            return connectionFactoryBeanName;
        }

        public static AcknowledgementMode ParseAcknowledgementMode(XmlElement element, ParserContext parserContext)
        {
            string acknowledge = element.GetAttribute(ACKNOWLEDGE_ATTRIBUTE);
            if (acknowledge.Equals(ACKNOWLEDGE_TRANSACTED))
            {
                return AcknowledgementMode.Transactional;
            }
            else if (acknowledge.Equals(ACKNOWLEDGE_DUPS_OK))
            {
                return AcknowledgementMode.DupsOkAcknowledge;
            }
            else if (acknowledge.Equals(ACKNOWLEDGE_CLIENT))
            {
                return AcknowledgementMode.ClientAcknowledge;
            }
            else if (!acknowledge.Equals(ACKNOWLEDGE_AUTO))
            {
                parserContext.ReaderContext.ReportException(element, ACKNOWLEDGE_ATTRIBUTE,
                                                            "Invalid listener container 'acknowledge' setting ['" +
                                                            acknowledge +
                                                            "]: only \"auto\", \"client\", \"dups-ok\" and \"transacted\" supported.");
            }
            return AcknowledgementMode.AutoAcknowledge;
        }
    }

}