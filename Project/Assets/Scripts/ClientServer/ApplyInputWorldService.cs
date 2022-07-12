using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Flow.EcsLite;

namespace Game.ClientServer
{
    /**
     * в конечном счете все изменения в InputWorld будут применяться тут
     * выполняется на клиенте и сервере, а также в синглплеерной игре.
     */

    public class ApplyInputWorldService: IInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, int tick, IInputComponent inp)
        {
            CreateInputEntity(inputWorld, playerId, tick, inp);
        }

        public static void CreateInputEntity(EcsWorld inputWorld, int playerId, int tick, IInputComponent inp)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<InputTickComponent>(inputWorld).Tick = tick;
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var pool = inputWorld.GetOrCreatePoolByType(inp.GetType());
            pool.AddRaw(inputEntity, inp);
        }
    }
}