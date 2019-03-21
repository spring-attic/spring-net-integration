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

using System;
using Spring.Integration.Util;

namespace Spring.Integration.Attributes {
    /// <summary>
    /// Indicates that a method is capable of mapping its parameters to a message
    /// or message payload. These method-level annotations are detected by the
    /// {@link org.springframework.integration.gateway.GatewayProxyFactoryObject}
    /// where the annotation attributes can override the default channel settings.
    /// 
    /// <p>A method annotated with @Gateway may accept a single non-annotated
    /// parameter of type {@link org.springframework.integration.core.Message}
    /// or of the intended Message payload type. Method parameters may be mapped
    /// to individual Message header values by using the {@link Header @Header}
    /// parameter annotation. Alternatively, to pass the entire Message headers
    /// map, a Map-typed parameter may be annotated with {@link Headers}.
    /// 
    /// <p>Return values from the annotated method may be of any type. If the
    /// declared return value is not a Message, the reply Message's payload will be
    /// returned and any type conversion as supported by Spring's
    /// {@link org.springframework.beans.SimpleTypeConverter} will be applied to
    /// the return value if necessary.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    [AttributeUsage(AttributeTargets.Method)]
    public class GatewayAttribute : Attribute {
        private string _requestChannel;
        private string _replyChannel;
        private TimeSpan _requestTimeout = TimeSpan.FromMilliseconds(-1);
        private TimeSpan _replyTimeout = TimeSpan.FromMilliseconds(-1);

        public string RequestChannel {
            get { return _requestChannel; }
            set { _requestChannel = value; }
        }

        public string ReplyChannel {
            get { return _replyChannel; }
            set { _replyChannel = value; }
        }

        public TimeSpan RequestTimeout {
            get { return _requestTimeout; }
            set { _requestTimeout = value; }
        }

        public TimeSpan ReplyTimeout {
            get { return _replyTimeout; }
            set { _replyTimeout = value; }
        }
    }
}
