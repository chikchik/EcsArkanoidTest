using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Physics;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;

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
            var poolPosition = inputWorld.GetPool<PositionComponent>();


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
                        if (inputMoveComponent.Dir.magnitude > 0)
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

                    if (poolPosition.Has(inputEntity))
                    {
                        var pos = poolPosition.Get(inputEntity).value; 
                        unitEntity.EntityReplace<PositionComponent>(world).value = pos;
                    }
                }
            }
        }

        public void Interract(EcsWorld world, int unitEntity)
        {
            var result = new List<int>();
            var position = unitEntity.EntityGet<PositionComponent>(world).value;
            
            world.GetNearestEntities(unitEntity,
                position,
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
                
                return;
            }
            
            world.GetNearestEntities(
                unitEntity,
                position,
                1, ref result, entity=> entity.EntityHas<RigidbodyComponent>(world));

            //if (result.Count > 0)
            {
                //var entity = result[0];
                
                unitEntity.EntityAdd<PushingComponent>(world);
                var kickEntity = world.NewEntity();
                ref var rb = ref kickEntity.EntityAdd<RigidbodyDefinitionComponent>(world);
                
                rb.BodyType = BodyType.Kinematic;
                rb.Density = 985f;
                rb.Friction = 0.3f;
                rb.Restitution = 0;
                rb.RestitutionThreshold = 0.5f;   

                ref var collider = ref kickEntity.EntityAddComponent<CircleColliderComponent>(world);
                collider.Radius = 0.1f;

                kickEntity.EntityAdd<PositionComponent>(world).value = position;
                kickEntity.EntityAdd<RotationComponent>(world);

                var lookDir = unitEntity.EntityGet<LookDirectionComponent>(world).value.normalized;
                kickEntity.EntityAdd<StartSimpleMoveAtComponent>(world).Time = world.GetTime() + 1f;
                var dir = lookDir * 12;
                kickEntity.EntityAdd<MoveSimpleDirectionComponent>(world).value = dir;
                kickEntity.EntityAdd<DestroyWhenTimeIsOutComponent>(world);
                kickEntity.EntityAdd<TimeComponent>(world).time = world.GetTime() + 1.1f;
            }

        }
    }
}