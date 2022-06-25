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
using Game.Ecs.ClientServer.Components.Input.Proto;
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
            
            var mainPlayerId = -1;
            if (world.HasUnique<MainPlayerIdComponent>())//если это мир на клиенте
                mainPlayerId = world.GetUnique<MainPlayerIdComponent>().value;
            

            var inputWorld = systems.GetWorld("input");
            
            var poolInputShot   = inputWorld.GetPool<InputShotComponent>();
            var poolPlayer      = inputWorld.GetPool<InputPlayerComponent>();
            var poolInputMoveDir= inputWorld.GetPool<InputMoveDirectionComponent>();
            var poolInputMoveTo = inputWorld.GetPool<InputMoveToPointComponent>();
            var poolInputAction = inputWorld.GetPool<InputActionComponent>();
            var poolInputTick = inputWorld.GetPool<InputTickComponent>();
            
            var filter = inputWorld.Filter<InputComponent>().End();

            var tick = world.GetTick();
            
            foreach (var inputEntity in filter)
            {
                if (poolInputTick.GetNullable(inputEntity)?.Tick != tick)
                    continue;
                
                var playerId = mainPlayerId;
                if (poolPlayer.Has(inputEntity))
                    playerId = poolPlayer.Get(inputEntity).PlayerID;

                var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerId);
                if (!world.IsEntityAliveInternal(unitEntity))
                {
                    Debug.LogError($"unit entity {unitEntity} is not alive");
                    continue;
                }

                if (!unitEntity.EntityHas<UnitComponent>(world))
                {
                    Debug.LogError($"entity {unitEntity} is not unit");
                    continue;
                }
                
                if (poolInputShot.Has(inputEntity))
                {
                    var dir = poolInputShot.Get(inputEntity).dir;
                    Shoot(world, unitEntity, dir);
                }

                if (poolInputMoveDir.Has(inputEntity))
                {
                    var dir = poolInputMoveDir.Get(inputEntity).Dir;
                    if (dir.sqrMagnitude > 0.001f)
                    {
                        unitEntity.EntityDel<TargetPositionComponent>(world);
                        unitEntity.EntityGetOrCreateRef<MoveDirectionComponent>(world).value = dir;
                    }
                    else
                    {
                        if (unitEntity.EntityHas<MoveDirectionComponent>(world))
                        {
                            unitEntity.EntityDel<MoveDirectionComponent>(world);
                            unitEntity.EntityDel<MovingComponent>(world);
                        }
                    }
                }

                if (poolInputMoveTo.Has(inputEntity))
                {
                    var inputMoveToPointComponent = poolInputMoveTo.Get(inputEntity);

                    ref var targetPositionComponent = ref unitEntity.EntityGetOrCreateRef<TargetPositionComponent>(world);
                    targetPositionComponent.Value = inputMoveToPointComponent.Value;
                }
                
                if (poolInputAction.Has(inputEntity))
                {
                    Interract(world, unitEntity);
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
                unitEntity.EntityGetOrCreateRef<FoodCollectedComponent>(world).Value += 1;
                ObjectiveService.Triggered(world, entity);

                if (entity.EntityHas<CollectableComponent>(world))
                {
                    entity.EntityGetRefComponent<CollectableComponent>(world).isCollected = true;
                }
                
                return;
            }
         
            if (unitEntity.EntityHas<PushingComponent>(world))
                return;
                
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
        
        private void Shoot(EcsWorld world, int unitEntity, Vector3 dir)
        {
            if (unitEntity.EntityHas<ShootingComponent>(world) &&
                !unitEntity.EntityGet<ShootingComponent>(world).ShootMade)
            {
                Debug.Log("skip");
                return;
            }

            world.Log($"shot at {world.GetTick()}");

            unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            unitEntity.EntityAdd<ShootStartedComponent>(world);
            unitEntity.EntityReplace(world, new ShootingComponent
            {
                Direction = dir, ShootAtTime = world.GetTime() + 0.2f, TotalTime = world.GetTime() + 0.5f
            });
        }
    }
}