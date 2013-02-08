using System;

namespace Spring.Integration.ChannelDemo
{
    public class Subscriber
    {
        private String _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Subscriber(string name)
        {
            _name = name;
        }

        public void Receive(string message)
        {
            Console.WriteLine("Subscriber:Receive(string) " + _name + ": Message = " + message);
        }

        public void Receive(int message)
        {
            Console.WriteLine("Subscriber:Recieve(int) " + _name + ": Message = " + message);
        }
    }
}