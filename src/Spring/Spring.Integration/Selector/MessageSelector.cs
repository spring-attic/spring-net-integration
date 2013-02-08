using Spring.Integration.Core;

namespace Spring.Integration.Selector {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate bool MessageSelectorAccept(IMessage message);

    public class MessageSelector : IMessageSelector {
        private MessageSelectorAccept _accept;

        public MessageSelector(MessageSelectorAccept accept) {
            _accept = accept;    
        }

        #region IMessageSelector Members

        public bool Accept(IMessage message) {
            return _accept(message);
        }

        #endregion
    }
}