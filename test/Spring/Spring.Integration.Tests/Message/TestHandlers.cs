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

namespace Spring.Integration.Tests.Message
{
    /// <summary>
    /// Factory for handler beans that are useful for testing.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Döhring (.NET)</author>
    public abstract class TestHandlers
    {
        ///**
        // * Create a handler that always returns null.
        // */
        //public static Delegate NullHandler() {
        //    return delegate(IMessage message) { return null; };
        //}

        ///**
        // * Create a handler that simply returns the {@link Message} it receives.
        // */
        //public final static object echoHandler() {
        //    return new object() {
        //        public Message<?> handle(Message<?> message) {
        //            return message;
        //        }
        //    };
        //}

        ///**
        // * Create a handler that increments the provided counter.
        // */
        //public final static object countingHandler(final AtomicInteger counter) {
        //    return new object() {
        //        public Message<?> handle(Message<?> message) {
        //            counter.incrementAndGet();
        //            return null;
        //        }
        //    };
        //}

        ///**
        // * Create a handler that counts down on the provided latch.
        // */
        //public final static object countDownHandler(final CountDownLatch latch) {
        //    return new object() {
        //        public Message<?> handle(Message<?> message) {
        //            latch.countDown();
        //            return null;
        //        }
        //    };
        //}

        ///**
        // * Create a handler that counts down on the provided latch
        // * and also increments the provided counter.
        // */
        //public final static object countingCountDownHandler(final AtomicInteger counter, final CountDownLatch latch) {
        //    return new object() {
        //        public Message<?> handle(Message<?> message) {
        //            counter.incrementAndGet();
        //            latch.countDown();
        //            return null;
        //        }
        //    };
        //}
    }
}