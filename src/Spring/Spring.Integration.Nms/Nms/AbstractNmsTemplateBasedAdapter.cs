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
using Apache.NMS;
using Spring.Messaging.Nms.Core;
using Spring.Messaging.Nms.Support.Destinations;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Nms
{
    /// <summary>
    /// Base class for adapters that delegate to a <see cref="NmsTemplate"/>
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Mark Pollack (.NET)</author>
    public abstract class AbstractNmsTemplateBasedAdapter : IInitializingObject
    {

	    private volatile IConnectionFactory connectionFactory;

	    private volatile IDestination destination;

	    private volatile String destinationName;

	    private volatile IDestinationResolver destinationResolver;

	    private volatile NmsTemplate nmsTemplate;

	    private volatile INmsHeaderMapper headerMapper;

	    private volatile bool initialized;

	    private readonly object initializationMonitor = new Object();


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNmsTemplateBasedAdapter"/> class.
        /// </summary>
        /// <param name="nmsTemplate">The NMS template.</param>
        public AbstractNmsTemplateBasedAdapter(NmsTemplate nmsTemplate)
        {
            this.nmsTemplate = nmsTemplate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNmsTemplateBasedAdapter"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="destination">The destination.</param>
        public AbstractNmsTemplateBasedAdapter(IConnectionFactory connectionFactory, IDestination destination)
        {
            this.connectionFactory = connectionFactory;
            this.destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNmsTemplateBasedAdapter"/> class.
        /// </summary>
        /// <param name="connectionFactory">The connection factory.</param>
        /// <param name="destinationName">Name of the destination.</param>
        public AbstractNmsTemplateBasedAdapter(IConnectionFactory connectionFactory, string destinationName)
        {
            this.connectionFactory = connectionFactory;
            this.destinationName = destinationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNmsTemplateBasedAdapter"/> class.
        /// </summary>
        public AbstractNmsTemplateBasedAdapter()
        {
        }

        /// <summary>
        /// Sets the connection factory.
        /// </summary>
        /// <value>The connection factory.</value>
        public IConnectionFactory ConnectionFactory
        {
            set { connectionFactory = value; }
        }

        /// <summary>
        /// Sets the destination.
        /// </summary>
        /// <value>The destination.</value>
        public IDestination Destination
        {
            set { destination = value; }
        }

        /// <summary>
        /// Sets the name of the destination.
        /// </summary>
        /// <value>The name of the destination.</value>
        public string DestinationName
        {
            set { destinationName = value; }
        }

        /// <summary>
        /// Sets the destination resolver.
        /// </summary>
        /// <value>The destination resolver.</value>
        public IDestinationResolver DestinationResolver
        {
            set { destinationResolver = value; }
        }

        /// <summary>
        /// Sets the NMS template.
        /// </summary>
        /// <value>The NMS template.</value>
        public NmsTemplate NmsTemplate
        {
            get
            {
                if (nmsTemplate == null)
                {
                    AfterPropertiesSet();
                }
                return nmsTemplate;
            }
            set { nmsTemplate = value; }
        }

        /// <summary>
        /// Sets the header mapper.
        /// </summary>
        /// <value>The header mapper.</value>
        public INmsHeaderMapper HeaderMapper
        {
            set { headerMapper = value; }
        }



        #region Implementation of IInitializingObject

        public void AfterPropertiesSet()
        {
            lock(initializationMonitor)
            {
                if (initialized)
                {
                    return;
                }
                if (nmsTemplate == null)
                {
                    AssertUtils.IsTrue(this.connectionFactory != null
                        && (this.destination != null || this.destinationName != null),
                        "Either a 'jmsTemplate' or *both* 'connectionFactory' and"
                        + " 'destination' (or 'destination-name') are required.");
                    nmsTemplate = CreateDefaultNmsTemplate();
                }
                ConfigureMessageConverter(nmsTemplate, headerMapper);
                initialized = true;
            }
        }

        #endregion


        private NmsTemplate CreateDefaultNmsTemplate()
        {
            NmsTemplate template = new NmsTemplate();
            template.ConnectionFactory = connectionFactory;
            if (destination != null)
            {
                template.DefaultDestination = destination;
            } else
            {
                template.DefaultDestinationName = destinationName;
            }
            if (destinationResolver != null)
            {
                template.DestinationResolver = destinationResolver;
            }
            return template;

        }

        protected abstract void ConfigureMessageConverter(NmsTemplate nmsTemplate, INmsHeaderMapper headerMapper);

    }

}