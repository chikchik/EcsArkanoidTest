
using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    /**
     * в конечном счете все изменения в InputWorld будут применяться тут
     * выполняется на клиенте и сервере, а также в синглплеерной игре.
     */

    public class ApplyInputWorldService: IInputService
    {
        public void Input(EcsWorld inputWorld, string playerId, int tick, IInputComponent inp)
        {
            CreateInputEntity(inputWorld, playerId, tick, inp);
        }
        
        public static void CreateInputEntity(EcsWorld inputWorld, string playerId, int tick, IInputComponent inp)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<InputTypeComponent>(inputWorld).Value = inp.GetType();
            inputEntity.EntityAdd<InputTickComponent>(inputWorld).Tick = tick;
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var pool = inputWorld.GetOrCreatePoolByType(inp.GetType());
            pool.AddRaw(inputEntity, inp);
        }
    }
}