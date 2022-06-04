using System;
using System.Collections.Generic;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;

using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyInputSystem : IEcsRunSystem
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
                
                return;
            }
         
                
            unitEntity.EntityAdd<PushingComponent>(world).EndTime = world.GetTime() + 1.3f;

            if (unitEntity.EntityHas<LookDirectionComponent>(world) && 
                !unitEntity.EntityHas<ApplyForceComponent>(world))
            {
                ref var component = ref unitEntity.EntityAdd<ApplyForceComponent>(world);
                component.Time = world.GetTime() + 1f;
                var dir = unitEntity.EntityGet<LookDirectionComponent>(world).value;
                var angle = Math.PI / 8f;
                var rotated = new Vector3();
                rotated.x = (float)(dir.x * Math.Cos(angle) - dir.z * Math.Sin(angle));
                rotated.z = (float)(dir.x * Math.Sin(angle) + dir.z * Math.Cos(angle));
                component.Direction = rotated;
            }
        }
    }
}