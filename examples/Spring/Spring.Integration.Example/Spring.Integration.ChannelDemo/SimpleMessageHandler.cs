using System;
using Spring.Integration.Core;
using Spring.Integration.Message;

namespace Spring.Integration.ChannelDemo
{
    public class SimpleMessageHandler  : IMessageHandler    
    {
        private String _name;
        public SimpleMessageHandler(string name)
        {
            _name = name;
        }

        #region Implementation of IMessageHandler

        public void HandleMessage(IMessage message)
        {
            Console.WriteLine("SimpleMessageHandler " + _name + ": Payload = " + message.Payload);
        }

        #endregion
    }
}