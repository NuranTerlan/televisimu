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
        static async Task Main(string[] args)
        {
            var id = Guid.NewGuid();
            Console.Title = "Udp Client (client@" + id + ')';

            var ipAddress = (await Dns.GetHostEntryAsync("localhost")).AddressList[0];

            // wait a little bit to guarantee that server is started
            // otherwise you'll get an exception about: an existing connection was forcibly closed by remote host (UDP-server)
            await Task.Delay(200);
            var portRead = AskForPort();
            
            var isPortInCorrectFormat = int.TryParse(portRead, out var port);
            if (!isPortInCorrectFormat)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Port is incorrect format!! (" + portRead + ')');
                Console.ResetColor();
            }

            var localEndPoint = new IPEndPoint(ipAddress, port);
            var client = new UdpClient(localEndPoint);
            var remoteEndPoint = new IPEndPoint(ipAddress, 5005);
            client.Connect(remoteEndPoint);

            var msgBytes = Encoding.ASCII.GetBytes("Hey server, it's client@" + id);
            await client.SendAsync(msgBytes, msgBytes.Length);

            while (true)
            {
                var receivedDgram = await client.ReceiveAsync();
                var buffer = receivedDgram.Buffer;
                var message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                WriteReceived(message);
            }
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
    }
}