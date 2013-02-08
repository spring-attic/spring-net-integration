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
using System.Threading;
using Spring.Threading;

namespace Spring.Integration.Util {

    /// <summary>
    /// Implementation of the JDK 1.5 {@link java.util.concurrent.ThreadFactory}
    /// interface, allowing for customizing the created threads (name, priority, etc).
    /// 
    /// <p>See the base class {@link org.springframework.util.CustomizableThreadCreator}
    /// for details on the available configuration options.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Andreas Döhring (.NET)</author>
    public class CustomizableThreadFactory : CustomizableThreadCreator, IThreadFactory {

        /// <summary>
        /// Create a new CustomizableThreadFactory with default thread name prefix.
        /// </summary>
        public CustomizableThreadFactory() {
        }

        /// <summary>
        /// Create a new CustomizableThreadFactory with the given thread name prefix.
        /// </summary>
        /// <param name="threadNamePrefix">the prefix to use for the names of newly created threads</param>
        public CustomizableThreadFactory(String threadNamePrefix)
            : base(threadNamePrefix) { }


        public Thread NewThread(IRunnable runnable) {
            return CreateThread(runnable);
        }
    }
}
