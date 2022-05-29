using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class JoinPlayerSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputJoinPlayerComponent>().End();

            foreach (var inputEntity in filter)
            {
                var joinPlayerComponent = inputEntity.EntityGetComponent<InputJoinPlayerComponent>(inputWorld);
                var playerID = joinPlayerComponent.playerID;
                var leave = joinPlayerComponent.leave;

                var ms = leave ? "leave" : "join";
                //context.WriteToConsole?.Invoke($"{ms} player {playerID}");
                if (leave)
                {
                    var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerID);
                    if (unitEntity != -1)
                        unitEntity.EntityWithRef(world, (ref PlayerComponent data) => { data.id = -1; });
                }
                else
                {
                    var freeUnitEntity = BaseServices.GetFreeUnitEntity(world);
                    if (freeUnitEntity != -1)
                        UnitService.ResetUnitEntity(world, freeUnitEntity);
                    else
                        freeUnitEntity = UnitService.CreateUnitEntity(world);

                    freeUnitEntity.EntityReplace<PlayerComponent>(world).id = playerID;
                }
            }
        }
    }
}