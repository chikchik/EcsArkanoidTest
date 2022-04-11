using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public static class BaseServices
    {
        public static int GetFreeUnitEntity(EcsWorld world)
        {
            var filter = world.Filter<LeoPlayerComponent>().End();
            foreach (var entity in filter)
            {
                var playerComponent = entity.EntityGetComponent<LeoPlayerComponent>(world);
                if (playerComponent.id == -1)
                    return entity;
            }

            return -1;
        }

        public static int GetUnitEntityByPlayerId(EcsWorld world, int playerID)
        {
            var poolPlayer = world.GetPool<LeoPlayerComponent>();

            var filter = world.Filter<LeoPlayerComponent>().End();
            foreach (var entity in filter)
                if (poolPlayer.Get(entity).id == playerID)
                    return entity;

            return -1;
        }

        public static void JoinPlayer(EcsWorld world, int playerID)
        {
            //Debug.Log($"joinPlayer {playerID}");
            var entity = world.NewEntity();
            entity.EntityAddComponent<InputJoinPlayerComponent>(world) = new InputJoinPlayerComponent
                {leave = false, playerID = playerID};
            entity.EntityAddComponent<InputComponent>(world);
        }

        public static void LeavePlayer(EcsWorld world, int playerID)
        {
            //Debug.Log($"leavePlayer {playerID}");
            var entity = world.NewEntity();
            entity.EntityAddComponent<InputJoinPlayerComponent>(world) = new InputJoinPlayerComponent
                {leave = true, playerID = playerID};
            entity.EntityAddComponent<InputComponent>(world);
        }
    }
}