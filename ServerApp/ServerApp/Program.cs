using System;
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
        static async Task Main(string[] args_)
        {
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12121));
            serverSocket.Listen(10);

            var provider = new ChannelProvider();
            provider.SetReliableSocket(serverSocket.Accept());

            var containerId = ContainerId.Parse("06f988be-52d8-461d-8283-03d280e2b1a5");
            Console.WriteLine($"Container id = {containerId.Value.ToString()}");
            var context = new ContainerStartingContext(
                containerId,
                new SimpleHost(new LoggerFactory(), provider),
                ContainerState.Empty
            );
            var container = await new ContainerFactory().StartContainerAsync(context);
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}