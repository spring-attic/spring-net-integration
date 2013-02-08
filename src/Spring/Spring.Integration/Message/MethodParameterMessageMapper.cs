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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Util;

namespace Spring.Integration.Message {
    /// <summary>
    /// Prepares arguments for handler methods. The method parameters are matched
    /// against the Message payload as well as its headers. If a method parameter
    /// is annotated with {@link Header @Header}, the annotation's value will be
    /// used as a header name. If such an annotation contains no value, then the
    /// parameter name will be used as long as the information is available in the
    /// class file (requires compilation with debug settings for parameter names).
    /// If the {@link Header @Header} annotation is not present, then the parameter
    /// will typically match the Message payload. However, if a Map or Properties
    /// object is expected, and the paylaod is not itself assignable to that type,
    /// then the MessageHeaders' values will be passed in the case of a Map-typed
    /// parameter, or the MessageHeaders' String-based values will be passed in the
    /// case of a Properties-typed parameter.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class MethodParameterMessageMapper : IInboundMessageMapper, IOutboundMessageMapper {

        private static ILog logger = LogManager.GetLogger(typeof (MethodParameterMessageMapper));

        private readonly MethodInfo _method;

        private volatile MethodParameterMetadata[] _parameterMetadata;

        public MethodParameterMessageMapper(MethodInfo method) {
            AssertUtils.ArgumentNotNull(method, "method must not be null");
            _method = method;
            InitializeParameterMetadata();
        }

        public IMessage ToMessage(object obj) {
            object[] parameters = obj as object[];
            if(parameters == null)
                throw new ArgumentNullException("parameters must not be null");
            if(parameters.Length == 0)
                throw new ArgumentException("Paramter ArrayUtils is required");

            if(parameters.Length != _method.GetParameters().Length)
                throw new ArgumentException("wrong number of parameters: expected " + _method.GetParameters().Length + ", received " + parameters.Length);

            IMessage message = null;
            object payload = null;
            IDictionary<string, object> headers = new Dictionary<string, object>();
            for(int i = 0; i < parameters.Length; ++i) {
                object value = parameters[i];

                MethodParameterMetadata metadata = _parameterMetadata[i];

                if(metadata.HeaderAttr != null) {
                    if(value != null) {
                        headers.Add(metadata.HeaderName, value);
                    }
                    else {
                        if(metadata.HeaderAttr.Required)
                            throw new ArgumentException("header '" + _parameterMetadata[i].HeaderName + "' is required");
                    }
                }
                else if(metadata.HasHeadersAnnotation) {
                    if(value != null) {
                        AddHeadersAnnotatedParameterToMap(value, headers);
                    }
                }
                else if(typeof(IMessage).IsAssignableFrom(metadata.ParameterType)) {
                    message = (IMessage)value;
                }
                else {
                    if(value == null)
                        throw new ArgumentException("payload object must not be null");
                    payload = value;
                }
            }
            if(message != null) {
                if(headers.Count == 0) {
                    return message;
                }
                return MessageBuilder.FromMessage(message).CopyHeadersIfAbsent(headers).Build();
            }
            if(payload == null)
                throw new ArgumentException("no parameter available for Message or payload");

            return MessageBuilder.WithPayload(payload).CopyHeaders(headers).Build();
        }

        public object FromMessage(IMessage message) {
            if(message == null) {
                return null;
            }
            if(message.Payload == null)
                throw new ArgumentException("Message payload must not be null.");

            object[] args = new object[_parameterMetadata.Length];
            for(int i = 0; i < _parameterMetadata.Length; i++) {
                MethodParameterMetadata metadata = this._parameterMetadata[i];
                Type expectedType = metadata.ParameterType;
                HeaderAttribute headerAttr = metadata.HeaderAttr;
                if(headerAttr != null) {
                    string headerName = metadata.HeaderName;
                    object value = message.Headers.Get(headerName);
                    if(value == null && headerAttr.Required) {
                        throw new MessageHandlingException(message, "required header '" + headerName + "' not available");
                    }
                    args[i] = value;
                }
                else if(metadata.HasHeadersAnnotation) {
                    if(typeof(Properties).IsAssignableFrom(expectedType)) {
                        args[i] = GetStringTypedHeaders(message);
                    }
                    else {
                        args[i] = message.Headers;
                    }
                }
                else if(expectedType.IsAssignableFrom(typeof(IMessage)) && typeof(IMessage).IsAssignableFrom(expectedType)) {
                    args[i] = message;
                }
                else {
                    args[i] = message.Payload;
                }
            }
            return args;
        }

        private void InitializeParameterMetadata() {
            bool foundMessageOrPayload = false;
            _parameterMetadata = new MethodParameterMetadata[_method.GetParameters().Length];
            for(int i = 0; i < _parameterMetadata.Length; i++) {
                MethodParameterMetadata metadata = new MethodParameterMetadata(_method, i);
                if(metadata.HeaderAttr == null && !metadata.HasHeadersAnnotation) {
                    // this is either a Message or the Object to be used as a Message payload
                    if(foundMessageOrPayload)
                        throw new ArgumentException("only one Message or payload parameter is allowed");
                    foundMessageOrPayload = true;
                }
                _parameterMetadata[i] = metadata;
            }
        }

        private Properties GetStringTypedHeaders(IMessage message) {
            Properties properties = new Properties();
            MessageHeaders headers = message.Headers;
            foreach(string key in headers.Keys) {
                object value = headers.Get(key);
                if(value is string) {
                    properties.SetProperty(key, (string)value);
                }
            }
            return properties;
        }

        //@SuppressWarnings("unchecked")
        private static void AddHeadersAnnotatedParameterToMap(object value, IDictionary<string, object> headers) {
            IDictionary dic = value as IDictionary;
            if(dic == null)
                throw new ArgumentException("parameter annotated with [Headers] must implement IDictionary");

            foreach(DictionaryEntry entry in dic) {
                string key = entry.Key as string;
                if(key == null)
                    throw new ArgumentException("parameter annotated with [Headers] must have string-typed keys");

                headers.Add((string)entry.Key, entry.Value);
            }
        }

        private class MethodParameterMetadata {

            private volatile HeaderAttribute _headerAnnotation;

            private volatile bool _hasHeadersAnnotation;

            private readonly ParameterInfo _parameterInfo;

            public MethodParameterMetadata(MethodInfo method, int index) {
                _parameterInfo = method.GetParameters()[index];
            }

            public HeaderAttribute HeaderAttr {
                get {
                    if(_headerAnnotation != null) {
                        return _headerAnnotation;
                    }

                    foreach(object attr in _parameterInfo.GetCustomAttributes(false)) {
                        if(attr is HeaderAttribute) {
                            _headerAnnotation = (HeaderAttribute)attr;
                            return _headerAnnotation;
                        }
                    }
                    return null;
                }
            }

            public string HeaderName {
                get {
                    if(HeaderAttr == null) {
                        return null;
                    }
                    string paramName = HeaderAttr.Value;
                    if(!StringUtils.HasText(paramName)) {
                        paramName = _parameterInfo.Name;
                        if(paramName == null)
                            throw new InvalidOperationException("No parameter name specified on [Header] and unable to reflect parameter name.");
                    }
                    return paramName;
                }
            }

            public bool HasHeadersAnnotation {
                get {
                    if(_hasHeadersAnnotation) {
                        return true;
                    }
                    foreach(object attr in _parameterInfo.GetCustomAttributes(false)) {
                        if(attr is HeadersAttribute) {
                            //Assert.isAssignable(Map.class,this.getParameterType(),"parameter with the @Headers annotation must be assignable to java.util.Map");
                            _hasHeadersAnnotation = true;
                            return true;
                        }
                    }
                    return false;
                }
            }

            public Type ParameterType {
                get {
                    return _parameterInfo.ParameterType;
                }
            }
        }
    }
}
