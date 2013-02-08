using Spring.Integration.Channel;

namespace Spring.Integration.ChannelDemo
{
    public class IAProducer
    {
        private IUIPublishSubscribeChannel channel;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public IAProducer(IUIPublishSubscribeChannel channel)
        {
            this.channel = channel;
        }

        public void Send()
        {
            channel.PublishObject("foo");
        }
    }
}