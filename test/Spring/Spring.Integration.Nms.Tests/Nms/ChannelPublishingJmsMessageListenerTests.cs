#region License

/*
 * Copyright 2002-2010 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Reflection;
using System.Threading;
using Apache.NMS;
using NUnit.Framework;
using Spring.Integration.Channel;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Messaging.Nms.Support.Converter;
using Spring.Threading;
using IMessage=Apache.NMS.IMessage;

namespace Spring.Integration.Nms
{
    [TestFixture]
    public class ChannelPublishingJmsMessageListenerTests
    {
        private StubSession session = new StubSession("test");
        [Test]
        [ExpectedException(typeof(InvalidDestinationException))]
        public void NoReplyToAndNoDefault()
        {
            QueueChannel requestChannel = new QueueChannel();
            StartBackgroundReplier(requestChannel);     
            ChannelPublishingJmsMessageListener listener = new ChannelPublishingJmsMessageListener();
            listener.ExpectReply = true;
            listener.RequestChannel = requestChannel;
            listener.MessageConverter = new TestMessageConverter();
            Apache.NMS.IMessage nmsMessage = session.CreateTextMessage("test");
            listener.AfterPropertiesSet();
            listener.OnMessage(nmsMessage, session);
        }

        [Test]
        public void DefaultHeaderMappingMessageConverter()
        {
            ChannelPublishingJmsMessageListener listener = new ChannelPublishingJmsMessageListener();
            listener.AfterPropertiesSet();
            HeaderMappingMessageConverter hmmc = GetHmmc(listener);
            Assert.That(hmmc, Is.Not.Null);
            SimpleMessageConverter smc = hmmc.GetType().GetField("converter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hmmc) as
                                         SimpleMessageConverter;
            Assert.That(smc, Is.Not.Null);
        }


        [Test]
        public void CustomMessageConverterDecoratedForHeaderMapping()
        {
            ChannelPublishingJmsMessageListener listener = new ChannelPublishingJmsMessageListener();
            IMessageConverter originalConverter = new TestMessageConverter();
            listener.MessageConverter = originalConverter;
            listener.AfterPropertiesSet();
            HeaderMappingMessageConverter hmmc = GetHmmc(listener);
            Assert.That(hmmc, Is.Not.Null);

            TestMessageConverter smc = hmmc.GetType().GetField("converter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(hmmc) as
                                       TestMessageConverter;
            Assert.That(smc, Is.Not.Null);

        }

        private HeaderMappingMessageConverter GetHmmc(ChannelPublishingJmsMessageListener listener)
        {
            FieldInfo field = listener.GetType().GetField("messageConverter", BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(listener) as HeaderMappingMessageConverter;
        }


        private void StartBackgroundReplier(QueueChannel channel)
        {


            new ThreadPerTaskExecutor().Execute(() =>
                                                    {
                                                        Spring.Integration.Core.IMessage request =
                                                            channel.Receive(TimeSpan.FromMilliseconds(5000));
                                                        Spring.Integration.Core.IMessage reply =
                                                            new StringMessage(((string) request.Payload).ToUpper());
                                                        ((IMessageChannel) request.Headers.ReplyChannel).Send(reply,
                                                                                                              TimeSpan.
                                                                                                                  FromMilliseconds
                                                                                                                  (5000));
                                                    });
        }

        /*
        public static Func<T, R> GetFieldAccessor<T, R>(string fieldName)
        {
            ParameterExpression param =
            Expression.Parameter(typeof(T), "arg");

            MemberExpression member =
            Expression.Field(param, fieldName);

            LambdaExpression lambda =
            Expression.Lambda(typeof(Func<T, R>), member, param);

            Func<T, R> compiled = (Func<T, R>)lambda.Compile();
            return compiled;
        }*/
    }

    public class TestMessageConverter : IMessageConverter
    {
        #region Implementation of IMessageConverter

        public IMessage ToMessage(object objectToConvert, ISession session)
        {
            return new StubTextMessage("test-to");
        }

        public object FromMessage(IMessage messageToConvert)
        {
            return "test-from";
        }

        #endregion
    }

}