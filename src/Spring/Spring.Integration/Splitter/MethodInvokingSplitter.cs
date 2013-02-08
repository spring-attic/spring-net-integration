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

using System.Reflection;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Handler;

namespace Spring.Integration.Splitter {
    /// <summary>
    /// A Message Splitter implementation that invokes the specified method
    /// on the given object. The method's return value will be split if it
    /// is a Collection or Array. If the return value is not a Collection or
    /// Array, then the single Object will be returned as the payload of a
    /// single reply Message.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MethodInvokingSplitter : AbstractMessageSplitter {

        private MessageMappingMethodInvoker _invoker;

        public MethodInvokingSplitter(object obj, MethodInfo method) {
            _invoker = new MessageMappingMethodInvoker(obj, method);
        }

        public MethodInvokingSplitter(object obj, string methodName) {
            _invoker = new MessageMappingMethodInvoker(obj, methodName);
        }

        public MethodInvokingSplitter(object obj) {
            _invoker = new MessageMappingMethodInvoker(obj, typeof(SplitterAttribute));
        }

        protected override object SplitMessage(IMessage message) {
            return _invoker.InvokeMethod(message);
        }
    }
}
