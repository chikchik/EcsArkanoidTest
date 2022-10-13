using System.Net;
using Gaming.ContainerManager.ImageContracts.V1;

namespace ServerApp.Server
{
    public class UnreliableUserAddress : IUserAddress
    {
        public string UserId { get; }
        public string ConnectionId { get; }

        public EndPoint EndPoint;

        public UnreliableUserAddress(string userId, string connectionId, EndPoint endPoint)
        {
            UserId = userId;
            ConnectionId = connectionId;
            EndPoint = endPoint;
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