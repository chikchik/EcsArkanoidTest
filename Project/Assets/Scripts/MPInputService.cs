using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;

namespace Game
{
    public class MPInputService: IInputService
    {
        private NetClient client;
        
        public MPInputService()
        {
        }

        public void SetNetClient(NetClient client)
        {
            this.client = client;
        }
        
        public void Input(EcsWorld inputWorld, int playerId, IInputComponent inp)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputOneFrameComponent>(inputWorld);
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var pool = inputWorld.GetOrCreatePoolByType(inp.GetType());
            pool.AddRaw(inputEntity, inp);

            client.AddUserInput(inp);
        }
    }
}