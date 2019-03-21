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

using System;
using System.Collections.Generic;
using Spring.Integration.Core;
using IMessage=Apache.NMS.IMessage;

namespace Spring.Integration.Nms.Config
{
    /// <summary>
    ///  
    /// </summary>
    /// <author>Mark Pollack</author>
    public class TestNmsHeaderMapper  : INmsHeaderMapper
    {
        #region Implementation of INmsHeaderMapper

        public void FromHeaders(MessageHeaders headers, IMessage nmsMessage)
        {
            
        }

        public IDictionary<string, object> ToHeaders(IMessage nmsMessage)
        {
            IDictionary<string, object> headerMap = new Dictionary<string, object>();
            headerMap.Add("testProperty", "foo");
            headerMap.Add("testAttribute", 123);
            return headerMap;
        }

        #endregion
    }

}