using System;
using Spring.Integration.Channel;

namespace Spring.Integration.ChannelDemo
{
    public class IASubscriber
    {
        private IUIPublishSubscribeChannel channel;
        private string _name;

        public IASubscriber(IUIPublishSubscribeChannel channel, string name)
        {
            this.channel = channel;
            _name = name;
        }

        public void Subscribe()
        {
            //channel.SubscribeObject<string>(Receive);
            //channel.SubscribeObject<string>(s => Console.WriteLine("IASubscriber Lambda " + _name + ": Message = " + s));
            channel.SubscribeObject<string>(s => Console.WriteLine("IASubscriber Lambda " + _name + ": Message = " + s),
                                            s => s.StartsWith("f"));
        }

        public void Receive(string message)
        {
            Console.WriteLine("IASubscriber " + _name + ": Message = " + message);
        }


    }
}