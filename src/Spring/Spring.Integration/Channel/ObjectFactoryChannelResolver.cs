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
using Spring.Integration.Core;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Util;

namespace Spring.Integration.Channel {

    /// <summary>
    /// <see cref="IChannelResolver"/> implementation based on a Spring <see cref="IObjectFactory"/>.
    ///
    /// <p>Will lookup Spring managed objects identified by object name,
    /// expecting them to be of type <see cref="IMessageChannel"/>.
    /// </summary>
    /// <author>Mark Fisher</author>
    /// <author>Andreas Doehring (.NET)</author>
    /// <see cref="IObjectFactory"/>
    public class ObjectFactoryChannelResolver : IChannelResolver, IObjectFactoryAware {

        private volatile IObjectFactory _objectFactory;

        /// <summary>
        /// Create a new instance of the <see cref="ObjectFactoryChannelResolver"/> class.
        /// <p>The ObjectFactory to access must be set via <see cref="ObjectFactory"/>.
        /// This will happen automatically if this resolver is defined within an
        /// ApplicationContext thereby receiving the callback upon initialization.
        /// <see cref="IObjectFactoryAware"/>
        /// </summary>
        public ObjectFactoryChannelResolver() {
        }

        /// <summary>
        /// Create a new instance of the <see cref="ObjectFactoryChannelResolver"/> class.
        /// <p>Use of this constructor is redundant if this object is being created
        /// by a Spring IoC container as the supplied <see cref="IObjectFactory"/> will be
        /// replaced by the <see cref="IObjectFactory"/> that creates it (c.f. the
        /// <see cref="IObjectFactoryAware"/> contract). So only use this constructor if you
        /// are instantiating this object explicitly rather than defining an object.
        /// </summary>
        /// <param name="objectFactory">the object factory to be used to lookup <see cref="IMessageChannel"/>s.</param>
        public ObjectFactoryChannelResolver(IObjectFactory objectFactory) {
            AssertUtils.ArgumentNotNull(objectFactory, "objectFactory", "objectFactory must not be null");
            _objectFactory = objectFactory;
        }

        #region IChannelResolver Members

        public IMessageChannel ResolveChannelName(string name) {
            if (_objectFactory == null)
                throw new ArgumentException("ObjectFactory is required");
            
            try {
                return (IMessageChannel) _objectFactory.GetObject(name, typeof (IMessageChannel));
            }
            catch (ObjectsException ex) {
                throw new ChannelResolutionException("failed to look up MessageChannel object with name [" + name + "]", ex);
            }
        }

        #endregion

        #region IObjectFactoryAware Members

        public IObjectFactory ObjectFactory {
            set { _objectFactory = value; }
        }
        
        #endregion
    }
}