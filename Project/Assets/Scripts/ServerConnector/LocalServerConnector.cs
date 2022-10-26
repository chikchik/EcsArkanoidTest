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
        private readonly IPAddress _ip;

        public LocalServerConnector(string ip, int tcpPort, int udpPort)
        {
            _ip = IPAddress.Parse("127.0.0.1");
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

            await socket.Connect(_ip, _tcpPort);
            Task.Run(socket.Run);

            return socket;
        }

        public async Task<ISocket> GetUnreliableConnection()
        {
            BaseSocket socket = new UnreliableSocket(_userId);

            await socket.Connect(_ip, _udpPort);
            Task.Run(socket.Run);

            return socket;
        }
    }
}