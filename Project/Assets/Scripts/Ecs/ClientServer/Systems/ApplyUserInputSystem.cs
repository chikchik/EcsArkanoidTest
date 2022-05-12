using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyUserInputSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputPlayerComponent>().End();

            var poolInputMove = inputWorld.GetPool<InputMoveComponent>();
            var poolInputMoveTo = inputWorld.GetPool<InputMoveToPointComponent>();
            var poolInputPlayer = inputWorld.GetPool<InputPlayerComponent>();
            var poolInputAction = inputWorld.GetPool<InputActionComponent>();


            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();


            foreach (var inputEntity in filter)
            {
                var playerID = poolInputPlayer.Get(inputEntity).PlayerID;
                var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerID);

                if (unitEntity != -1)
                {
                    if (poolInputMove.Has(inputEntity))
                    {
                        var inputMoveComponent = poolInputMove.Get(inputEntity);

                        poolMoveDirection.GetRef(unitEntity).value = inputMoveComponent.Dir;
                        poolLookDirection.GetRef(unitEntity).value = inputMoveComponent.Dir;

                        unitEntity.EntityDel<TargetPositionComponent>(world);

                    }

                    if (poolInputAction.Has(inputEntity))
                    {
                        Interract(world, unitEntity);
                    }

                    if (poolInputMoveTo.Has(inputEntity))
                    {
                        var inputMoveToPointComponent = poolInputMoveTo.Get(inputEntity);

                        ref var targetPositionComponent = ref unitEntity.EntityReplace<TargetPositionComponent>(world);
                        targetPositionComponent.Value = inputMoveToPointComponent.Value;
                    }
                }
            }
        }

        public void Interract(EcsWorld world, int unitEntity)
        {
            var result = new List<int>();
            world.GetNearestEntities(
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref result, entity=> entity.EntityHas<InteractableComponent>(world));

            if (result.Count > 0)
            {
                var entity = result[0];
                entity.EntityDel<InteractableComponent>(world);
                unitEntity.EntityReplace<FoodCollectedComponent>(world).Value += 1;
                ObjectiveService.Triggered(world, entity);

                if (entity.EntityHas<CollectableComponent>(world))
                {
                    entity.EntityGetRefComponent<CollectableComponent>(world).isCollected = true;
                }
            }
        }
    }
}