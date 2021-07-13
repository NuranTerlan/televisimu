using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class UdpServer
    {
        public IPAddress IpAddress { get; }
        public IPEndPoint LocalEndPoint { get; }
        public UdpClient UdpSocket { get; }
        public ConcurrentBag<IPEndPoint> ClientEndPoints { get; }

        private readonly IPHostEntry _hostEntry = Dns.GetHostEntry("localhost");

        public UdpServer(int port)
        {
            IpAddress = _hostEntry.AddressList[0];
            LocalEndPoint = new IPEndPoint(IpAddress, port);
            UdpSocket = new UdpClient(LocalEndPoint);
            ClientEndPoints = new ConcurrentBag<IPEndPoint>();

            Task.Run(async () => await AcceptRequestingClient());
        }

        private async Task AcceptRequestingClient()
        {
            while (true)
            {
                var received = await UdpSocket.ReceiveAsync();
                var buffer = received.Buffer;
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                ClientEndPoints.Add(received.RemoteEndPoint);
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
    }
}