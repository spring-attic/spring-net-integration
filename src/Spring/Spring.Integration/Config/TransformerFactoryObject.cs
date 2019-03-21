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

using Spring.Integration.Message;
using Spring.Integration.Splitter;
using Spring.Integration.Transformer;
using Spring.Util;

namespace Spring.Integration.Config {
    /// <summary>
    /// Factory bean for creating a Message Transformer.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class TransformerFactoryBean : AbstractMessageHandlerFactoryObject {

        protected override IMessageHandler CreateHandler(object targetObject, string targetMethodName) {
            AssertUtils.ArgumentNotNull(targetObject, "targetObject must not be null");
            ITransformer transformer = null;
            if(targetObject is ITransformer) {
                transformer = (ITransformer)targetObject;
            }
            else if(StringUtils.HasText(targetMethodName)) {
                transformer = new MethodInvokingTransformer(targetObject, targetMethodName);
            }
            else {
                transformer = new MethodInvokingTransformer(targetObject);
            }
            return new MessageTransformingHandler(transformer);
        }
    }
}
