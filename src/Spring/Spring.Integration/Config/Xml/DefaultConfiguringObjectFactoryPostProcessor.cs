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
using Apache.NMS;
using Common.Logging;
using Spring.Integration.Context;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Threading;

namespace Spring.Integration.Config.Xml {
    /// <summary>
    /// A {@link ObjectFactoryPostProcessor} implementation that provides default
    /// beans for the error handling and task scheduling if those beans have not
    /// already been explicitly defined within the registry.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas D�hring (.NET)</author>
    public class DefaultConfiguringObjectFactoryPostProcessor : IObjectFactoryPostProcessor {

        private readonly ILog logger = LogManager.GetLogger(typeof(DefaultConfiguringObjectFactoryPostProcessor));

        public void PostProcessObjectFactory(IConfigurableListableObjectFactory objectFactory) {
            
            IObjectDefinitionRegistry registry = objectFactory as IObjectDefinitionRegistry;
            if(registry != null) {
                RegisterNullChannel(registry);
                RegisterErrorChannelIfNecessary(registry);
                RegisterTaskSchedulerIfNecessary(registry);
            }
            else if(logger.IsWarnEnabled) {
                logger.Warn("ObjectFactory is not a ObjectDefinitionRegistry. The default '"
                        + IntegrationContextUtils.ErrorChannelObjectName + "' and '"
                        + IntegrationContextUtils.TaskSchedulerObjectName + "' cannot be configured.");
            }
        }

        private void RegisterNullChannel(IObjectDefinitionRegistry registry)
        {
            if (registry.IsObjectNameInUse(IntegrationContextUtils.NullChannelObjectName))
            {
                throw new IllegalStateException("The object name '" + IntegrationContextUtils.NullChannelObjectName 
                    + "' is reserved.");
            }
            RootObjectDefinition nullChannelDef = new RootObjectDefinition();
            nullChannelDef.ObjectTypeName = IntegrationNamespaceUtils.BASE_PACKAGE + ".Channel.NullChannel";
            ObjectDefinitionHolder nullChannelHolder = new ObjectDefinitionHolder(nullChannelDef, IntegrationContextUtils.NullChannelObjectName);
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(nullChannelHolder, registry);
               
        }

        /**
         * Register an error channel in the given ObjectDefinitionRegistry if not yet present.
         * The bean name for which this is checking is defined by the constant
         * {@link IntegrationContextUtils#ERROR_CHANNEL_BEAN_NAME}.
         */
        private void RegisterErrorChannelIfNecessary(IObjectDefinitionRegistry registry) {
            if(!registry.ContainsObjectDefinition(IntegrationContextUtils.ErrorChannelObjectName)) {
                if(logger.IsInfoEnabled) {
                    logger.Info("No bean named '" + IntegrationContextUtils.ErrorChannelObjectName +
                            "' has been explicitly defined. Therefore, a default PublishSubscribeChannel will be created.");
                }

                RootObjectDefinition errorChannelDef = new RootObjectDefinition();
                errorChannelDef.ObjectTypeName = IntegrationNamespaceUtils.BASE_PACKAGE + ".Channel.PublishSubscribeChannel";
                ObjectDefinitionHolder errorChannelHolder = new ObjectDefinitionHolder(errorChannelDef, IntegrationContextUtils.ErrorChannelObjectName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(errorChannelHolder, registry);
                             
                ObjectDefinitionBuilder loggingHandlerBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.HANDLER_PACKAGE + ".LoggingHandler");

                string loggingHandlerObjectName = ObjectDefinitionReaderUtils.GenerateObjectName(loggingHandlerBuilder.ObjectDefinition, registry);

                loggingHandlerBuilder.AddConstructorArg("ERROR");
                ObjectDefinitionHolder loggingHandlerHolder = new ObjectDefinitionHolder(loggingHandlerBuilder.ObjectDefinition, loggingHandlerObjectName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(loggingHandlerHolder, registry);

                ObjectDefinitionBuilder loggingEndpointBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.ENDPOINT_PACKAGE + ".EventDrivenConsumer");
                loggingEndpointBuilder.AddConstructorArgReference(IntegrationContextUtils.ErrorChannelObjectName);
                loggingEndpointBuilder.AddConstructorArgReference(loggingHandlerObjectName);
                string loggingEndpointObjectName =ObjectDefinitionReaderUtils.GenerateObjectName(loggingEndpointBuilder.ObjectDefinition, registry);
                ObjectDefinitionHolder loggingEndpointHolder = new ObjectDefinitionHolder(loggingEndpointBuilder.ObjectDefinition, loggingEndpointObjectName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(loggingEndpointHolder, registry);
            }
        }

        /// <summary>
        /// Register a TaskScheduler in the given <see cref="IObjectDefinitionFactory"/> if not yet present.
        /// The object name for which this is checking is defined by the constant <see cref="IntegrationContextUtils.TaskSchedulerObjectName"/>
        /// </summary>
        /// <param name="registry">the <see cref="IObjectDefinitionFactory"/></param>
        private void RegisterTaskSchedulerIfNecessary(IObjectDefinitionRegistry registry) {
            if(!registry.ContainsObjectDefinition(IntegrationContextUtils.TaskSchedulerObjectName)) {
                if(logger.IsInfoEnabled) {
                    logger.Info("No object named '" + IntegrationContextUtils.TaskSchedulerObjectName +
                            "' has been explicitly defined. Therefore, a default SimpleTaskScheduler will be created.");
                }
                IExecutor taskExecutor = IntegrationContextUtils.CreateThreadPoolTaskExecutor(2, 100, 0, "task-scheduler-");
                ObjectDefinitionBuilder schedulerBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.SCHEDULING_PACKAGE + ".SimpleTaskScheduler");
                schedulerBuilder.AddConstructorArg(taskExecutor);
                
                ObjectDefinitionBuilder errorHandlerBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(IntegrationNamespaceUtils.CHANNEL_PACKAGE + ".MessagePublishingErrorHandler");
                errorHandlerBuilder.AddPropertyReference("defaultErrorChannel", IntegrationContextUtils.ErrorChannelObjectName);
                string errorHandlerBeanName = ObjectDefinitionReaderUtils.GenerateObjectName(errorHandlerBuilder.ObjectDefinition, registry);
                ObjectDefinitionHolder errorHandlerHolder = new ObjectDefinitionHolder(errorHandlerBuilder.ObjectDefinition, errorHandlerBeanName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(errorHandlerHolder, registry);

                schedulerBuilder.AddPropertyReference("errorHandler", errorHandlerBeanName);
                ObjectDefinitionHolder schedulerHolder = new ObjectDefinitionHolder(schedulerBuilder.ObjectDefinition, IntegrationContextUtils.TaskSchedulerObjectName);
                ObjectDefinitionReaderUtils.RegisterObjectDefinition(schedulerHolder, registry);
            }
        }
    }
}
