using System.Threading.Tasks;
using Gaming.ContainerManager.Client.SocketContracts.V1;
using XFlow.Net.Client;

namespace Game
{
    public class ServerConnector : IServerConnector
    {
        public ServerConnector()
        {
        }

        public async Task<ISocket> GetReliableConnection()
        {
            return null;
        }
        
        public async Task<ISocket> GetUnreliableConnection()
        {
            return null;
        }
    }
}