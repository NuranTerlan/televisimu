using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class UdpServer
    {
        public IPAddress IpAddress { get; }
        public IPEndPoint LocalEndPoint { get; }
        public UdpClient UdpSocket { get; }
        public HashSet<IPEndPoint> ClientEndPoints { get; }

        private readonly IPHostEntry _hostEntry = Dns.GetHostEntry("localhost");

        public UdpServer(int port)
        {
            IpAddress = _hostEntry.AddressList[1];
            LocalEndPoint = new IPEndPoint(IpAddress, port);
            UdpSocket = new UdpClient(LocalEndPoint);
            ClientEndPoints = new HashSet<IPEndPoint>();

            InformSuccessfulStartingOnConsole("Server is running on: " + LocalEndPoint);
            
            Task.Run(async () => await AcceptRequestingClient());
        }

        private async Task AcceptRequestingClient()
        {
            while (true)
            {
                var received = await UdpSocket.ReceiveAsync();
                var buffer = received.Buffer;
                var remoteEndPoint = received.RemoteEndPoint;
                var message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                Console.WriteLine(message);
                if (Regex.IsMatch(message, "out"))
                {
                    ClientEndPoints.Remove(remoteEndPoint);
                    continue;
                }
                
                ClientEndPoints.Add(remoteEndPoint);
            }
        }

        public async Task SendContinuousDatagrams(IEnumerable<string> words)
        {
            var datagrams = words.Select(w => Encoding.ASCII.GetBytes(w));
            while (true)
            {
                await IterateDatagrams(datagrams);
            }
        }

        private async Task IterateDatagrams(IEnumerable<byte[]> datagrams)
        {
            foreach (var dgram in datagrams)
            {
                await SendIterationElementToAllClients(dgram);
            }
        }

        private async Task SendIterationElementToAllClients(byte[] dgram)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            Parallel.ForEach(ClientEndPoints, async (ep) =>
            {
                await UdpSocket.SendAsync(dgram, dgram.Length, ep);
            });


        }
        
        private static void InformSuccessfulStartingOnConsole(string content)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(content + '\n');
            Console.ResetColor();
        }
    }
}