using System;
using System.Reflection;
using System.Threading;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Handler
{
    public class ActionHandler<T> : AbstractReplyProducingMessageHandler
    {

        private Action<T> _action;

        private readonly SynchronizationContext _synchronizationContext;

        public ActionHandler(Action<T> action) {
            _action = action;
        }

        public ActionHandler(Action<T> action, SynchronizationContext synchronizationContext)
        {
            _action = action;
            _synchronizationContext = synchronizationContext;
        }

        #region Overrides of AbstractReplyProducingMessageHandler

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder)
        {
            try
            {
                object result = null;
                if (_synchronizationContext == null)
                {
                    //PayloadFilter ensures this cast will work.
                    _action.Invoke((T)message.Payload);
                } else
                {
                    _synchronizationContext.Send( state => _action.Invoke((T)message.Payload), null );
                }
                if (result != null)
                {
                    replyHolder.Set(result);
                }
            }
            catch (Exception e)
            {
                if (e is SystemException)
                {
                    throw;
                }
                throw new MessageHandlingException(message, "failure occurred in Service Activator '" + this + "'", e);
            }
        }

        #endregion
    }
}