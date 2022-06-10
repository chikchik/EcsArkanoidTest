using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;

namespace Game.ClientServer
{
    public class SingleInputService: IInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, IInputComponent inp)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var pool = inputWorld.GetOrCreatePoolByType(inp.GetType());
            pool.AddRaw(inputEntity, inp);
        }
    }
}