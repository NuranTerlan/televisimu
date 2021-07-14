using System;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "UDP-Server";

            var server = new UdpServer(5005);
            var sentWords = new string[]
            {
                "The Last of Us II",
                "Days Gone",
                "Far Cry IV",
                "Miracle",
                "Kafka",
                "Server",
                "Client",
                "Udp",
                "Tcp",
                "Port",
                "Ip"
            };
            await server.SendContinuousDatagrams(sentWords);
        }
    }
}