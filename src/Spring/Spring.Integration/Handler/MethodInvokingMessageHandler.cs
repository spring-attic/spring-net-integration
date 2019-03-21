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
using System.Reflection;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Handler {
    /// <summary>
    /// A {@link MessageHandler} that invokes the specified method on the provided object.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MethodInvokingMessageHandler : MessageMappingMethodInvoker, IMessageHandler {

        public MethodInvokingMessageHandler(object obj, MethodInfo method)
            : base(obj, method) {
            
            if(!method.ReturnType.Equals(typeof(void)))
                throw new ArgumentException("MethodInvokingMessageHandler requires a void-returning method");
        }

        public MethodInvokingMessageHandler(object obj, string methodName)
            : base(obj, methodName) {

        }


        public void HandleMessage(IMessage message) {
            object result = this.InvokeMethod(message);
            if(result != null && !result.GetType().Equals(typeof(Missing))) {
                throw new MessagingException(message, "the MethodInvokingMessageHandler method must "
                        + "have a void return, but '" + this + "' received a value: [" + result + "]");
            }
        }
    }
}
