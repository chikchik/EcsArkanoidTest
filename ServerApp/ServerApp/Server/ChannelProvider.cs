using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class ChannelProvider : IChannelProvider
    {
        private Socket _tcpSocket;
        private Socket _udpSocket;

        public void SetReliableSocket(Socket socket)
        {
            _tcpSocket = socket;
        }

        public void SetUnreliableSocket(Socket socket)
        {
            _udpSocket = socket;
        }

        public async ValueTask<IReliableChannel> GetReliableChannelAsync()
        {
            var channel = new ReliableChannel(_tcpSocket);
            channel.Start();
            return channel;
        }


        public async ValueTask<IUnreliableChannel> GetUnreliableChannelAsync()
        {
            var channel = new UnreliableChannel(_udpSocket);
            channel.Start();
            return channel;
        }
    }
}