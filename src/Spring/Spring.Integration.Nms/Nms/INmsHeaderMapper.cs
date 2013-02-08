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

using System;
using System.Collections.Generic;
using Spring.Integration.Core;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// Strategy interface for mapping integration Message headers to an outbound
    /// NMS Message (e.g. to configure NMS properties) or extracting integration
    /// header values from an inbound NMS Message.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface INmsHeaderMapper
    {
        /// <summary>
        /// Froms the headers.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <param name="nmsMessage">The NMS message.</param>
        void FromHeaders(MessageHeaders headers, Apache.NMS.IMessage nmsMessage);

        /// <summary>
        /// Convertsthe headers.
        /// </summary>
        /// <param name="nmsMessage">The NMS message.</param>
        /// <returns>Header map</returns>
        IDictionary<string, object> ToHeaders(Apache.NMS.IMessage nmsMessage);
    }

}