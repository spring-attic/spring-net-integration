//#region License

///*
// * Copyright 2002-2009 the original author or authors.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// *      http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF Any KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */

//#endregion

//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;
//using Spring.Context;
//using Spring.Integration.Aggregator;
//using Spring.Integration.Attributes;
//using Spring.Integration.Channel;
//using Spring.Integration.Core;
//using Spring.Integration.Endpoint;
//using Spring.Integration.Message;
//using Spring.Integration.Tests.Util;
//using Spring.Objects.Factory.Support;
//using Spring.Threading.Helpers;

//namespace Spring.Integration.Tests.Config {
//    /// <author>Mark Fisher</author>
//    /// <author>Andreas Döhring (.NET)</author>
//    [TestFixture]
//public class ServiceActivatorAnnotationPostProcessorTests {

//    [Test]
//    public void TestAnnotatedMethod() {
//        CountDownLatch latch = new CountDownLatch(1);
//        TestUtils.TestApplicationContext context = TestUtils.CreateTestApplicationContext();
//        RootObjectDefinition postProcessorDef = new RootObjectDefinition(MessagingAnnotationPostProcessor.class);
//        context.registerBeanDefinition("postProcessor", postProcessorDef);
//        context.registerBeanDefinition("testChannel", new RootBeanDefinition(DirectChannel.class));
//        RootBeanDefinition beanDefinition = new RootBeanDefinition(SimpleServiceActivatorAnnotationTestBean.class);
//        beanDefinition.getConstructorArgumentValues().addGenericArgumentValue(latch);
//        context.registerBeanDefinition("testBean", beanDefinition);
//        context.refresh();
//        SimpleServiceActivatorAnnotationTestBean testBean = (SimpleServiceActivatorAnnotationTestBean) context.getBean("testBean");
//        assertEquals(1, latch.getCount());
//        assertNull(testBean.getMessageText());
//        MessageChannel testChannel = (MessageChannel) context.getBean("testChannel");
//        testChannel.send(new StringMessage("test-123"));
//        latch.await(1000, TimeUnit.MILLISECONDS);
//        assertEquals(0, latch.getCount());
//        assertEquals("test-123", testBean.getMessageText());
//        context.stop();
//    }


//    public class AbstractServiceActivatorAnnotationTestBean {

//        protected string _messageText;

//        private readonly CountDownLatch _latch;

//        public AbstractServiceActivatorAnnotationTestBean(CountDownLatch latch) {
//            _latch = latch;
//        }

//        protected void CountDown() {
//            _latch.CountDown();
//        }

//        public string MessageText {
//            get { return _messageText; }
//        }
//    }


//    [MessageEndpoint]
//    public class SimpleServiceActivatorAnnotationTestBean : AbstractServiceActivatorAnnotationTestBean {

//        public SimpleServiceActivatorAnnotationTestBean(CountDownLatch latch) 
//            : base(latch)
//        {}

//        [ServiceActivator(InputChannel="testChannel")]
//        public void TestMethod(string messageText) {
//            _messageText = messageText;
//            CountDown();
//        }
//    }
//}
//}
