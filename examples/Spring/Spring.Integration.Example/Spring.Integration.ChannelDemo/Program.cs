using System;
using Spring.Integration.Channel;
using Spring.Integration.Config.Xml;
using Spring.Integration.Core;
using Spring.Integration.Message;
using Spring.Objects.Factory.Xml;


namespace Spring.Integration.ChannelDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //SimpleMessageHandlerEventing();
            //InfrastructureAwareEventing();
            PocoSubscriberEventing();
           

            Console.ReadKey();
        }
        
        private static void SimpleMessageHandlerEventing()
        {
            PublishSubscribeChannel channel = new PublishSubscribeChannel();
            channel.Subscribe(new SimpleMessageHandler("one"));
            channel.Subscribe(new SimpleMessageHandler("two"));
            IMessage message = MessageBuilder.WithPayload("foo").Build();
            channel.Send(message);   
            
        }

        private static void InfrastructureAwareEventing()
        {
            IUIPublishSubscribeChannel channel = new UIPublishSubscribeChannel();
            IAProducer iaProducer = new IAProducer(channel);
            IASubscriber sub1 = new IASubscriber(channel, "one");
            IASubscriber sub2 = new IASubscriber(channel, "two");
            sub1.Subscribe();
            sub2.Subscribe();
            iaProducer.Send();
        }

        private static void PocoSubscriberEventing()
        {
            IUIPublishSubscribeChannel channel = new UIPublishSubscribeChannel();
            //IAProducer iaProducer = new IAProducer(channel);
            Subscriber sub1 = new Subscriber("one");
            Subscriber sub2 = new Subscriber("two");
            channel.SubscribeObject<string>(sub1);
            channel.SubscribeObject<int>(sub2);
            channel.PublishObject("foo");
            channel.PublishObject(2);
        }
    }
}
