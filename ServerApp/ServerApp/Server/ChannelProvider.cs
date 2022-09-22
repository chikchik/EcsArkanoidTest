using System.Net.Sockets;
using System.Threading.Tasks;
using Gaming.ContainerManager.ImageContracts.V1.Channels;

namespace ServerApp.Server
{
    public class ChannelProvider : IChannelProvider
    {
        private Socket _socket;

        public void SetReliableSocket(Socket socket)
        {
            _socket = socket;
        }

        public async ValueTask<IReliableChannel> GetReliableChannelAsync()
        {
            var channel = new Reliable(_socket);
            channel.Start();
            return channel;
        }


        public async ValueTask<IUnreliableChannel> GetUnreliableChannelAsync()
        {
            return new Unreliable();
        }
    }
}