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

namespace Spring.Integration.Attributes {
    /// <summary>
    /// Annotation indicating that a method parameter's value should be
    /// retrieved from the message headers. The value of the annotation
    /// provides the header name, and the optional 'required' property
    /// specifies whether the attribute value must be available within
    /// the header. The default value for 'required' is <code>true</code>.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// /// <author>Andreas Döhring (.NET)</author>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class HeaderAttribute : Attribute {
        private bool _required = true;
        private string _value;

        public HeaderAttribute() {
        }

        public HeaderAttribute(string value) {
            _value = value;
        }

        public bool Required {
            get { return _required; }
            set { _required = value; }
        }

        public string Value {
            get { return _value; }
            set { _value = value; }
        }
    }
}