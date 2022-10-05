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
            return UserId.Equals(other.UserId);
        }
    }
}