using Game.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;

namespace Game.UI
{
    public static class ClientBaseServices
    {
        public static void SetPlayerEntity(EcsWorld world, int playerEntity)
        {
            world.GetOrCreateUniqueRef<ClientPlayerEntityComponent>().Value = world.PackEntity(playerEntity);
        }
        
        public static bool TryGetPlayerEntity(EcsWorld world, out int entity)
        {
            entity = -1;
            if (!world.TryGetUnique(out ClientPlayerEntityComponent data))
                return false;

            return data.Value.Unpack(world, out entity);
        }
        
        public static bool TryGetControlledEntity(EcsWorld world, out int controlledEntity)
        {
            int playerEntity = -1;
            controlledEntity = -1;
            if (!TryGetPlayerEntity(world, out playerEntity))
                return false; 
            
            return BaseServices.TryGetControlledEntity(world, playerEntity, out controlledEntity);
        }

        public static bool IsControlledEntity(EcsWorld world, int entity)
        {
            if (!TryGetControlledEntity(world, out int controlledEntity))
                return false;
            return controlledEntity == entity;
        }
    }
}