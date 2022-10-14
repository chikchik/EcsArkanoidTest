using Gaming.ContainerManager.ImageContracts.V1;
using XFlow.Net.ClientServer.Ecs.Components;

namespace XFlow.Server.Components
{
    public struct UserAddressComponent
    {
        public IUserAddress Address;
    }
    public struct UserConnectedInputComponent
    {
    }
    public struct UserDisconnectedInputComponent
    {
    }
}