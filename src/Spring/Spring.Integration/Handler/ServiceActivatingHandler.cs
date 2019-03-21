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
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Handler {
    /// <summary>
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class ServiceActivatingHandler : AbstractReplyProducingMessageHandler {

        private MessageMappingMethodInvoker invoker;


        public ServiceActivatingHandler(object obj) {
            this.invoker = new MessageMappingMethodInvoker(obj, typeof(ServiceActivatorAttribute));
        }

        public ServiceActivatingHandler(object obj, MethodInfo method) {
            this.invoker = new MessageMappingMethodInvoker(obj, method);
        }

        public ServiceActivatingHandler(object obj, string methodName) {
            this.invoker = new MessageMappingMethodInvoker(obj, methodName);
        }


        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder) {
            try {
                object result = this.invoker.InvokeMethod(message);
                if(result != null) {
                    replyHolder.Set(result);
                }
            }
            catch(Exception e) {
                if(e is SystemException) {
                    throw;
                }
                throw new MessageHandlingException(message, "failure occurred in Service Activator '" + this + "'", e);
            }
        }

        public override string ToString() {
            return "ServiceActivator for [" + this.invoker + "]";
        }
    }
}
