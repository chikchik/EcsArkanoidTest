using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Leopotam.EcsLite;
using UnityEngine;

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
                        unitEntity.EntityWithRef(world, (ref LeoPlayerComponent data) => { data.id = -1; });
                }
                else
                {
                    var freeUnitEntity = BaseServices.GetFreeUnitEntity(world);
                    if (freeUnitEntity != -1)
                        UnitService.ResetUnitEntity(world, freeUnitEntity);
                    else
                        freeUnitEntity = UnitService.CreateUnitEntity(world);
                    
                    freeUnitEntity.EntityReplace<LeoPlayerComponent>(world).id = playerID;
                }
            }
        }
    }
}