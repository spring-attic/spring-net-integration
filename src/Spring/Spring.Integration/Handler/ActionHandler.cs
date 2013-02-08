using System;
using System.Reflection;
using System.Threading;
using Spring.Integration.Attributes;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.Handler
{
    public class UIServiceActivatingHandler : AbstractReplyProducingMessageHandler
    {

        private MessageMappingMethodInvoker invoker;

        private readonly SynchronizationContext synchronizationContext;

        public UIServiceActivatingHandler(object obj)
        {
            this.invoker = new MessageMappingMethodInvoker(obj, typeof(ServiceActivatorAttribute));
        }

        public UIServiceActivatingHandler(object obj, MethodInfo method) {
            this.invoker = new MessageMappingMethodInvoker(obj, method);
        }

        public UIServiceActivatingHandler(object obj, string methodName, SynchronizationContext synchronizationContext) {
            this.invoker = new MessageMappingMethodInvoker(obj, methodName);
        }

        #region Overrides of AbstractReplyProducingMessageHandler

        protected override void HandleRequestMessage(IMessage message, ReplyMessageHolder replyHolder)
        {
            try
            {
                object result = null;
                if (synchronizationContext == null)
                {
                    result = this.invoker.InvokeMethod(message);
                } else
                {
                    synchronizationContext.Send( state => invoker.InvokeMethod(message), null );
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