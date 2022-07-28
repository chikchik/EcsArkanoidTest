
using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Input;
using XFlow.Utils;

namespace Game.ClientServer.Services
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
        
        public static int GetControlledEntity(EcsWorld world, int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                int mechEntity;
                if (unitEntity.EntityGet<ControlsMechComponent>(world).PackedEntity.Unpack(world, out mechEntity))
                    return mechEntity;
            }

            return unitEntity;
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