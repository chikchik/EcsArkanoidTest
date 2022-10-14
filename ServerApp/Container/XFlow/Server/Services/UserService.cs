using Gaming.ContainerManager.ImageContracts.V1;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Server.Components;
using XFlow.Utils;

namespace XFlow.Server.Services
{
    public static class UserService
    {
        public static void InputUserDisconnected(EcsWorld inputWorld, IUserAddress userAddress)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<UserDisconnectedInputComponent>(inputWorld);
            inputEntity.EntityAdd<UserAddressComponent>(inputWorld).Address = userAddress;
        }
        
        public static void InputUserConnected(EcsWorld inputWorld, in ClientComponent client)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<UserConnectedInputComponent>(inputWorld);
            inputEntity.EntityAdd<ClientComponent>(inputWorld) = client;
        }
    }
}