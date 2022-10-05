using Gaming.ContainerManager.ImageContracts.V1;

namespace ServerApp.Server
{
    public class UserAddress : IUserAddress
    {
        public string UserId { get; }
        public string ConnectionId { get; }

        public UserAddress(string userId)
        {
            UserId = userId;
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