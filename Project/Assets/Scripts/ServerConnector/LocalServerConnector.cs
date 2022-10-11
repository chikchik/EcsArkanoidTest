using System.Net;
using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.Net.Client;

namespace Game
{
    public class LocalServerConnector : IServerConnector
    {
        private readonly int _tcpPort;
        private readonly int _udpPort;

        private readonly int _userId;

        public LocalServerConnector(int tcpPort, int udpPort)
        {
            _tcpPort = tcpPort;
            _udpPort = udpPort;

            _userId = UnityEngine.Random.Range(1000, 9999);
        }

        public async Task<string> GetUserId()
        {
            return _userId.ToString();
        }

        public async Task<ISocket> GetReliableConnection()
        {
            BaseSocket socket = new ReliableSocket(_userId);

            await socket.Connect(IPAddress.Parse("127.0.0.1"), _tcpPort);
            socket.Run();

            return socket;
        }

        public async Task<ISocket> GetUnreliableConnection()
        {
            BaseSocket socket = new UnreliableSocket(_userId);

            await socket.Connect(IPAddress.Parse("127.0.0.1"), _udpPort);
            socket.Run();

            return socket;
        }
    }
}