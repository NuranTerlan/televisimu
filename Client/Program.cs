using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        private static readonly Guid Id = Guid.NewGuid();
        private static readonly IPAddress IpAddress = Dns.GetHostEntry("localhost").AddressList[1];
        private static readonly IPEndPoint ServerEndPoint = new IPEndPoint(IpAddress, 5005);
        private static readonly UdpClient Client = new UdpClient(0);
        private static readonly IPEndPoint LocalEndPoint = (IPEndPoint) Client.Client.LocalEndPoint;

        public Program()
        {
        }
        
        static async Task Main(string[] args)
        {
            Console.Title = "UDP-Client (client@" + Id + ')';

            // wait a little bit to guarantee that server is started
            // otherwise you'll get an exception about: an existing connection was forcibly closed by remote host (UDP-server)
            await Task.Delay(TimeSpan.FromSeconds(2));

            // var portRead = AskForPort();
            // var isPortInCorrectFormat = int.TryParse(portRead, out var port);
            // if (!isPortInCorrectFormat)
            // {
            //     Console.ForegroundColor = ConsoleColor.Red;
            //     Console.WriteLine("Port is incorrect format!! (" + portRead + ')');
            //     Console.ResetColor();
            // }

            InformSuccessfulStartingOnConsole($"UDP-Client client@{Id} is started on: {LocalEndPoint}");
            Client.Connect(ServerEndPoint);

            var msgBytes = Encoding.ASCII.GetBytes("Hey server, it's client@" + Id + " | from " + LocalEndPoint);
            await Client.SendAsync(msgBytes, msgBytes.Length);
            
            Task.Run(async () => await WaitForExitKey());
            
            while (true)
            {
                var receivedDgram = await Client.ReceiveAsync();
                var buffer = receivedDgram.Buffer;
                var message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                WriteReceived(message);
            }
        }

        private static void InformSuccessfulStartingOnConsole(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(content + '\n');
            Console.ResetColor();
        }

        private static void WriteReceived(string content)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("RECEIVED: ");
            Console.ResetColor();
            Console.Write(content.PadRight(25));
            Console.WriteLine(DateTime.UtcNow.ToLocalTime().ToString("yyyy-M-d HH-mm-ss.fff"));
        }

        private static string AskForPort()
        {
            Console.Write("Enter port for udp client: ");
            var input = Console.ReadLine() ?? "1111";
            Console.WriteLine();

            return input;
        }

        private static async Task WaitForExitKey()
        {
            if (Console.ReadKey().Key == ConsoleKey.Escape)
            {
                var successful = await SendExitRequestToServer();
                if (successful)
                {
                    Environment.Exit(0);
                }
            }
        }

        private static async Task<bool> SendExitRequestToServer()
        {
            try
            {
                var msgBytes = Encoding.ASCII.GetBytes("client@" + Id + " from " + LocalEndPoint + " is out..");
                await Client.SendAsync(msgBytes, msgBytes.Length);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Can't send exit request to the server. Error content: " + e.Message);
                return false;
            }
        }
    }
}