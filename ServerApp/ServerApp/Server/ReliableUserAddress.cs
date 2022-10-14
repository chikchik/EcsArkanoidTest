using System.Net.Sockets;
using Gaming.ContainerManager.ImageContracts.V1;

namespace ServerApp.Server
{
    public class ReliableUserAddress : IUserAddress
    {
        public string UserId { get; }
        public string ConnectionId { get; }

        public Socket Socket;
        
        public ReliableUserAddress(string userId, string connectionId, Socket socket)
        {
            UserId = userId;
            ConnectionId = connectionId;
            Socket = socket;
        }

        public bool Equals(IUserAddress other)
        {
            if (other == null)
                return false;
            return UserId == other.UserId && ConnectionId == other.ConnectionId;
        }

        public override string ToString()
        {
            return $"{UserId} [{ConnectionId}]";
        }
    }
}