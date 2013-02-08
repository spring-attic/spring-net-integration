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
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#region

using Spring.Integration.Core;

#endregion

namespace Spring.Integration.Nms
{
    /// <summary>
    /// Pre-defined names and prefixes to be used for setting and/or retrieving NMS
    /// attributes from/to integration Message Headers.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public sealed class NmsHeaders
    {
        /// <summary>
        /// Prefix used for NMS API related headers in order to distinguish from
	    /// user-defined headers and other internal headers (e.g. correlationId).
	    /// <see cref="DefaultJmsHeaderMapper"/>
        /// </summary>
        public const string PREFIX = MessageHeaders.PREFIX + "nms_";

        public const string MESSAGE_ID = PREFIX + "messageId";

        public const string CORRELATION_ID = PREFIX + "correlationId";

        public const string REPLY_TO = PREFIX + "replyTo";

        public const string REDELIVERED = PREFIX + "redelivered";

        public const string TYPE = PREFIX + "type";
    }
}