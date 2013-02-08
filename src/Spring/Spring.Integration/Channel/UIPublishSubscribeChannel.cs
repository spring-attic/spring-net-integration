using System;
using System.Collections.Generic;
using System.Threading;
using Spring.Integration.Core;
using Spring.Integration.Filter;
using Spring.Integration.Handler;
using Spring.Integration.Message;
using Spring.Integration.Selector;

namespace Spring.Integration.Channel
{
    public interface IUIPublishSubscribeChannel
    {
        /// <summary>
        /// Convenience method to sends the object.
        /// </summary>
        /// <param name="objectToSend">The object to send.</param>
        /// <returns>
        /// <code>true</code> if the message is sent successfully or
        /// <code>false</code> if the sending thread is interrupted.
        /// </returns>
        bool PublishObject(object objectToSend);

        void SubscribeObject<T>(object subscriber);
        void SubscribeObject<T>(object subscriber, Predicate<T> subscriberFilter);
        void SubscribeObject<T>(object subscriber, Predicate<T> subscriberFilter, SynchronizationContext synchronizationContext);
        void SubscribeObject<T>(Action<T> action);
        void SubscribeObject<T>(Action<T> action, Predicate<T> subscriberFilter);
        void SubscribeObject<T>(Action<T> action, Predicate<T> subscriberFilter, SynchronizationContext synchronizationContext);
    }

    public class UIPublishSubscribeChannel : PublishSubscribeChannel, IUIPublishSubscribeChannel
    {

        /// <summary>
        /// Convenience method to sends the object.
        /// </summary>
        /// <param name="objectToSend">The object to send.</param>
        /// <returns>
        /// <code>true</code> if the message is sent successfully or
        /// <code>false</code> if the sending thread is interrupted.
        /// </returns>
        public bool PublishObject(object objectToSend)
        {
            IMessage message = MessageBuilder.WithPayload(objectToSend).Build();
            return Send(message);
        }


        public void SubscribeObject<T>(object subscriber)
        {
            SubscribeObject<T>(subscriber, null, SynchronizationContext.Current);
        }

        public void SubscribeObject<T>(object subscriber, Predicate<T> subscriberFilter)
        {
            SubscribeObject<T>(subscriber, subscriberFilter, SynchronizationContext.Current);
        }

        public void SubscribeObject<T>(object subscriber, Predicate<T> subscriberFilter, SynchronizationContext synchronizationContext)
        {
            MessageHandlerChain chain = new MessageHandlerChain();
            IMessageSelector messageSelector = new PayloadTypeSelector(typeof(T));
            IMessageSelector predecateSelector = new PredicateSelector<T>(subscriberFilter);
            
            UIServiceActivatingHandler serviceActivatingHandler = new UIServiceActivatingHandler(subscriber, "Receive", synchronizationContext);

            IList<IMessageHandler> handlerList = new List<IMessageHandler>();
            handlerList.Add(new MessageFilter(messageSelector));
            handlerList.Add(new MessageFilter(predecateSelector));
            handlerList.Add(serviceActivatingHandler);
            chain.Handlers = handlerList;

            Subscribe(chain);

        }

        public void SubscribeObject<T>(Action<T> action)
        {
            SubscribeObject(action, null, SynchronizationContext.Current);
        }

        public void SubscribeObject<T>(Action<T> action, Predicate<T> subscriberFilter)
        {
            SubscribeObject(action, subscriberFilter, SynchronizationContext.Current);
        }

        public void SubscribeObject<T>(Action<T> action, Predicate<T> subscriberFilter, SynchronizationContext synchronizationContext)
        {
            MessageHandlerChain chain = new MessageHandlerChain();
            IMessageSelector messageSelector = new PayloadTypeSelector(typeof(T));
            IMessageSelector predecateSelector = new PredicateSelector<T>(subscriberFilter);

            ActionHandler<T> actionHandler = new ActionHandler<T>(action, synchronizationContext);

            IList<IMessageHandler> handlerList = new List<IMessageHandler>();
            handlerList.Add(new MessageFilter(messageSelector));
            handlerList.Add(new MessageFilter(predecateSelector));
            handlerList.Add(actionHandler);
            chain.Handlers = handlerList;

            Subscribe(chain);
        }

        public class PredicateSelector<T> : IMessageSelector
        {
            private Predicate<T> _predicate;
            public PredicateSelector(Predicate<T> predicate)
            {
                _predicate = predicate;
            }

            #region Implementation of IMessageSelector

            public bool Accept(IMessage message)
            {
                if (_predicate == null)
                {
                    return false;
                }
                return _predicate((T)message.Payload);
            }

            #endregion
        }

    }
}