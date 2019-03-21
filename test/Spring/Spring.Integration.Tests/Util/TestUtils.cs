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

#region

using System;
using System.Reflection;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Integration.Config.Xml;
using Spring.Integration.Context;
using Spring.Integration.Core;
using Spring.Integration.Endpoint;
using Spring.Integration.Scheduling;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading;
using Spring.Threading.Collections.Generic;
using Spring.Threading.Execution;
using Spring.Util;

#endregion

namespace Spring.Integration.Tests.Util
{
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public abstract class TestUtils
    {
        public static IApplicationContext GetContext(params string[] files)
        {
            NamespaceParserRegistry.RegisterParser(typeof (IntegrationNamespaceParser));

            return new XmlApplicationContext(files);
        }

        public static object GetFieldValue(object root, string fieldName)
        {
            for (Type type = root.GetType(); type != null; type = type.BaseType)
            {
                FieldInfo fieldInfo = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                    return fieldInfo.GetValue(root);
            }
            throw new ArgumentException("cannot find field [" + fieldName + "] in [" + root.GetType() + "]");
        }

        public static T GetFieldValue<T>(object root, string fieldName)
        {
            object value = GetFieldValue(root, fieldName);
            Assert.That(typeof (T).IsAssignableFrom(value.GetType()), Is.True);
            return (T) value;
        }

        public static object GetPropertyValue(object root, string propertyPath)
        {
            object value = root;
            string[] tokens = propertyPath.Split('.');
            for (int i = 0; i < tokens.Length; i++)
            {
                PropertyInfo pi = value.GetType().GetProperty(tokens[i],
                                                              BindingFlags.NonPublic | BindingFlags.Instance |
                                                              BindingFlags.Public);
                if (pi != null)
                {
                    value = pi.GetValue(value, null);
                }
                else
                {
                    throw new ArgumentException("property '" + tokens[i] + "' not found in [" + value.GetType() + "]");
                }
            }
            return value;
        }

        public static T GetPropertyValue<T>(object root, string propertyPath)
        {
            object value = GetPropertyValue(root, propertyPath);
            if (value != null)
                Assert.That(typeof (T).IsAssignableFrom(value.GetType()), Is.True);
            return (T) value;
        }

        public static TestApplicationContext CreateTestApplicationContext()
        {
            TestApplicationContext context = new TestApplicationContext();
            RegisterObject(IntegrationContextUtils.TaskSchedulerObjectName, CreateTaskScheduler(10), context);
            return context;
        }

        public static ITaskScheduler CreateTaskScheduler(int poolSize)
        {
            ThreadPoolExecutor executor = new ThreadPoolExecutor(
                poolSize, poolSize, new TimeSpan(0, 0, 10), new LinkedBlockingQueue<IRunnable>(), new ThreadPoolExecutor.CallerRunsPolicy()
                );
            return new SimpleTaskScheduler(executor);
        }

        private static void RegisterObject(string objectName, object obj, IObjectFactory objectFactory)
        {
            AssertUtils.ArgumentNotNull(objectName, "object name must not be null");
            IConfigurableListableObjectFactory configurableListableObjectFactory = null;

            if (objectFactory is IConfigurableListableObjectFactory)
            {
                configurableListableObjectFactory = (IConfigurableListableObjectFactory) objectFactory;
            }
            else if (objectFactory is GenericApplicationContext)
            {
                configurableListableObjectFactory = ((GenericApplicationContext) objectFactory).ObjectFactory;
            }
            if (obj is IObjectNameAware)
            {
                ((IObjectNameAware) obj).ObjectName = objectName;
            }
            if (obj is IObjectFactoryAware)
            {
                ((IObjectFactoryAware) obj).ObjectFactory = objectFactory;
            }
            if (obj is IInitializingObject)
            {
                try
                {
                    ((IInitializingObject) obj).AfterPropertiesSet();
                }
                catch (Exception ex)
                {
                    throw new FatalObjectException("failed to register bean with test context", ex);
                }
            }
            if (configurableListableObjectFactory == null)
                throw new ArgumentException("configurableListableObjectFactory must not be null");

            configurableListableObjectFactory.RegisterSingleton(objectName, obj);
        }


        public class TestApplicationContext : GenericApplicationContext
        {
            public void RegisterChannel(string channelName, IMessageChannel channel)
            {
                if (channel.Name != null)
                {
                    if (channelName == null)
                    {
                        AssertUtils.ArgumentNotNull(channel.Name, "channel.Name", "channel name must not be null");
                        channelName = channel.Name;
                    }
                    else
                    {
                        AssertUtils.IsTrue(channel.Name.Equals(channelName),
                                           "channel name has already been set with a conflicting value");
                    }
                }
                RootObjectDefinition rod = new RootObjectDefinition(channel.GetType());

                RegisterObjectDefinition(channelName, rod); //, this);
            }

            public void RegisterEndpoint(string endpointName, AbstractEndpoint endpoint)
            {
                if (endpoint is AbstractPollingEndpoint)
                {
                    //DirectFieldAccessor accessor = new DirectFieldAccessor(endpoint);
                    //if (accessor.getPropertyValue("trigger") == null) {
                    ((AbstractPollingEndpoint) endpoint).Trigger = new IntervalTrigger(new TimeSpan(0, 0, 0, 0, 10));
                    //}
                }
                RegisterObject(endpointName, endpoint, this);
            }
        }
    }
}