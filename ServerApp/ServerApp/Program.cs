using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1;
using Gaming.ContainerManager.Models.V1;
using ServerApp.Server;
using XFlow.Server;

namespace ServerApp2
{
    internal class Program
    {
        static async Task Main(string[] _)
        {
            var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            tcpSocket.Bind(new IPEndPoint(IPAddress.Any, 12001));
            tcpSocket.Listen(10);

            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, 12002));

            var provider = new ChannelProvider();
            provider.SetReliableSocket(tcpSocket);
            provider.SetUnreliableSocket(udpSocket);

            var containerId = ContainerId.Parse("06f988be-52d8-461d-8283-03d280e2b1a5");
            var context = new ContainerStartingContext(
                containerId,
                new SimpleHost(new LoggerFactory(), provider),
                null,
                ContainerState.Empty
            );
            
            var container = await new ContainerFactory().StartContainerAsync(context);
            while (true)
            {
                Thread.Sleep(100);
                // var info = await container.GetInfoAsync();
                // context.Host.LoggerFactory.System.Log(LogLevel.Debug, info);
            }
        }
    }
}