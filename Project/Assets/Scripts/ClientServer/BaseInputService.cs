using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Input;
using Leopotam.EcsLite;

namespace Game.ClientServer
{
    public class BaseInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, IInputComponent inp)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var tp = inp.GetType();
            var pool = inputWorld.GetOrCreatePoolByType(tp);
            pool.AddRaw(inputEntity, inp);
        }
    }
}