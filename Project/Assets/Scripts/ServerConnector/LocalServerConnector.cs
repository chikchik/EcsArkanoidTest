using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.Net.Client;

namespace Game
{
    public class LocalServerConnector : IServerConnector
    {
        private readonly int _tcpPort;
        private readonly int _udpPort;

        public LocalServerConnector(int tcpPort, int udpPort)
        {
            _tcpPort = tcpPort;
            _udpPort = udpPort;
        }

        public async Task<ISocket> GetReliableConnection()
        {
            var socket = new ReliableSocket();
            socket.Connect(IPAddress.Parse("127.0.0.1"), _tcpPort);
            socket.Run();

            return socket;
        }

        public async Task<ISocket> GetUnreliableConnection()
        {
            var socket = new UnreliableSocket();
            socket.Connect(IPAddress.Parse("127.0.0.1"), _udpPort);
            socket.Run();

            return socket;
        }
    }
}