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

namespace Spring.Integration.Example.IntervalOddEven {
    /// <author>Mark Fisher</author>
    /// <author>Marius Bogoevici</author>
    /// <author>Andreas Doehring (.NET)</author>
    //@MessageEndpoint
    public class EvenLogger {

        //@ServiceActivator
        public void Log(int i) {
            Console.WriteLine("even: " + i + " at " + DateTime.Now);
            //new SimpleDateFormat("yyyy-MM-dd hh:mm:ss").format(new Date()));
        }
    }
}
