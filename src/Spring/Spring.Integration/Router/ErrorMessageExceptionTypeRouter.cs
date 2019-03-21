//#region License

///*
// * Copyright 2002-2009 the original author or authors.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *      https://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */

//#endregion

//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using Spring.Integration.Core;

//namespace Spring.Integration.Router {
//    /// <summary>
//    /// A Message Router that resolves the target {@link MessageChannel} for
//    /// messages whose payload is an Exception. The channel resolution is based upon
//    /// the most specific cause of the error for which a channel-mapping exists.
//    /// </summary>
//    /// <author>Mark Fisher</author>
//    /// <author>Andreas D�hring (.NET)</author>
//public class ErrorMessageExceptionTypeRouter : AbstractSingleChannelRouter {

//    private volatile Map<Class<? extends Throwable>, MessageChannel> exceptionTypeChannelMap =
//            new ConcurrentHashMap<Class<? extends Throwable>, MessageChannel>();


//    public void setExceptionTypeChannelMap(Map<Class<? extends Throwable>, MessageChannel> exceptionTypeChannelMap) {
//        Assert.notNull(exceptionTypeChannelMap, "exceptionTypeChannelMap must not be null");
//        this.exceptionTypeChannelMap = exceptionTypeChannelMap;
//    }


//    @Override
//    protected MessageChannel determineTargetChannel(Message<?> message) {
//        MessageChannel channel = null;
//        object payload = message.getPayload();
//        if (payload != null && (payload instanceof Throwable)) {
//            Throwable mostSpecificCause = (Throwable) payload;
//            while (mostSpecificCause != null) {
//                MessageChannel mappedChannel = this.exceptionTypeChannelMap.get(mostSpecificCause.getClass());
//                if (mappedChannel != null) {
//                    channel = mappedChannel;
//                }
//                mostSpecificCause = mostSpecificCause.getCause();
//            }
//        }
//        return channel;
//    }

//}
