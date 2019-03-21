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

using System.Collections;
using System.Reflection;
using Spring.Integration.Attributes;
using Spring.Integration.Channel.Interceptor;
using Spring.Integration.Core;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Util;

namespace Spring.Integration.Transformer {
    /// <author>Jonas Partner</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class MethodInvokingTransformer : ITransformer {

        private MessageMappingMethodInvoker _invoker;


        public MethodInvokingTransformer(object obj, MethodInfo method) {
            _invoker = new MessageMappingMethodInvoker(obj, method);
        }

        public MethodInvokingTransformer(object obj, string methodName) {
            _invoker = new MessageMappingMethodInvoker(obj, methodName);
        }

        public MethodInvokingTransformer(object obj) {
            _invoker = new MessageMappingMethodInvoker(obj, typeof(TransformerAttribute));
        }


        public IMessage Transform(IMessage message) {
            object result = _invoker.InvokeMethod(message);
            if(result is IMessage) {
                return (IMessage)result;
            }
            if(result is Properties && !(message.Payload is Properties)) {
                Properties propertiesToSet = (Properties)result;
                MessageBuilder builder = MessageBuilder.FromMessage(message);
                foreach(object keyObject in propertiesToSet.Keys) {
                    string key = (string)keyObject;
                    builder.SetHeader(key, propertiesToSet.GetProperty(key));
                }
                return builder.Build();
            }
            if(result is IDictionary && !(message.Payload is IDictionary)) {
                IDictionary attributesToSet = (IDictionary)result;
                MessageBuilder builder = MessageBuilder.FromMessage(message);
                foreach(object key in attributesToSet.Keys) {
                    if(!(key is string)) {
                        throw new MessageHandlingException(message, "Map returned from a Transformer method must have String-typed keys");
                    }
                    builder.SetHeader((string)key, attributesToSet[key]);
                }
                return builder.Build();
            }
            return MessageBuilder.WithPayload(result).CopyHeaders(message.Headers).Build();
        }
    }
}
