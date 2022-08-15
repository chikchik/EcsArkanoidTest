using System;
using System.Collections.Generic;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Inventory;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Other;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Net.ClientServer.Ecs.Components.Input.proto;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyInputSystem : IEcsInitSystem, IEcsRunSystem
    {
        EcsFilter filter;
        private EcsWorld world;
        private EcsWorld inputWorld;

        private MyInventoryService inventoryService;
        
        List<int> entities = new List<int>();

        public ApplyInputSystem(MyInventoryService myInventoryService)
        {
            inventoryService = myInventoryService;
        }
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            inputWorld = systems.GetWorld("input");
            filter = inputWorld.Filter<InputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var mainPlayerId = -1;
            if (world.HasUnique<MainPlayerIdComponent>())//если это мир на клиенте
                mainPlayerId = world.GetUnique<MainPlayerIdComponent>().value;
           
            
            var poolInputShot   = inputWorld.GetPool<InputShotComponent>();
            var poolPlayer      = inputWorld.GetPool<InputPlayerComponent>();
            var poolInputMoveDir= inputWorld.GetPool<InputMoveDirectionComponent>();
            var poolInputMoveTo = inputWorld.GetPool<InputMoveToPointComponent>();
            var poolMoveItem = inputWorld.GetPool<MoveItemComponent>();
            var poolClearInventory = inputWorld.GetPool<ClearInventoryComponent>();
            var poolInputAction = inputWorld.GetPool<InputActionComponent>();
            var poolInputKick   = inputWorld.GetPool<InputKickComponent>();
            var poolInputTick   = inputWorld.GetPool<InputTickComponent>();
            
            
            

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
                    Shoot(unitEntity, poolInputShot.Get(inputEntity));
                }

                if (poolInputMoveDir.Has(inputEntity))
                {
                    Move(unitEntity, poolInputMoveDir.Get(inputEntity).Dir);
                }

                if (poolInputMoveTo.Has(inputEntity))
                {
                    MoveToPoint(unitEntity, poolInputMoveTo.Get(inputEntity).Value);
                }
                
                if (poolInputAction.Has(inputEntity))
                {
                    Interract(world, unitEntity);
                }
                
                if (poolInputKick.Has(inputEntity))
                {
                    var dir = poolInputKick.Get(inputEntity).dir;
                    Kick(unitEntity, dir);
                }

                if (inputEntity.EntityHas<InputMechEnterLeaveComponent>(inputWorld))
                {
                    EnterLeaveMech(unitEntity);
                }

                if (inputEntity.EntityHas<MoveItemComponent>(inputWorld))
                {
                    MoveItem(poolMoveItem.Get(inputEntity));
                }

                if (inputEntity.EntityHas<ClearInventoryComponent>(inputWorld))
                {
                    ClearInventory(poolClearInventory.Get(inputEntity));
                }
            }
        }

        public void EnterLeaveMech(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                unitEntity.EntityDel<ControlsMechComponent>(world);
                return;
            }
            
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref entities, entity=> entity.EntityHas<MechComponent>(world));

            if (entities.Count == 0)
                return;
            
            var entity = entities[0];
            ref var packedEntity = ref unitEntity.EntityAdd<ControlsMechComponent>(world).PackedEntity;
            packedEntity = world.PackEntity(entity);
        }
        
        public void Interract(EcsWorld world, int unitEntity)
        {
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref entities, entity=> entity.EntityHas<InteractableComponent>(world));

            if (entities.Count == 0)
                return;
            
            var entity = entities[0];
            if (entity.EntityHas<SpawnGunComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<WeaponComponent>(world);
                TryHideCollected(entity);
            }
            
            if (entity.EntityHas<BushComponent>(world))
            {
                entity.EntityDel<InteractableComponent>(world);
                unitEntity.EntityGetOrCreateRef<FoodCollectedComponent>(world).Value += 1;
                ObjectiveService.Triggered(world, entity);


                if (entity.EntityHas<CollectableComponent>(world))
                {
                    entity.EntityGetRef<CollectableComponent>(world).isCollected = true;
                }

                if (!unitEntity.EntityHas<InventoryLinkComponent>(world))
                {
                    return;
                }

                var inventoryLink = unitEntity.EntityGet<InventoryLinkComponent>(world);
                if (!inventoryLink.Inventory.Unpack(world, out var inventory))
                {
                    return;
                }

                var berryEntity = world.NewEntity();
                berryEntity.EntityAdd<AmountComponent>(world).Value = 1;
                berryEntity.EntityAdd<ItemIdComponent>(world).ItemId = "Berry";

                inventoryService.Add(world, inventory, berryEntity);
            }

            if (entity.EntityHas<AmmoComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<AmmoCollectedComponent>(world).Value += 10;
                entity.EntityDel<InteractableComponent>(world);
                TryHideCollected(entity);
            }
        }

        void TryHideCollected(int entity)
        {
            if (entity.EntityHas<CollectableComponent>(world))
            {
                entity.EntityGetRef<CollectableComponent>(world).isCollected = true;
            }
        }
        
        public void Kick(int unitEntity, Vector3 dir)
        {
            if (unitEntity.EntityHas<PushingComponent>(world))
                return;
                
            unitEntity.EntityAdd<PushingComponent>(world).EndTime = world.GetTime() + 1.3f;
            unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            
            if (unitEntity.EntityHas<LookDirectionComponent>(world) && 
                !unitEntity.EntityHas<ApplyForceComponent>(world))
            {
                ref var component = ref unitEntity.EntityAdd<ApplyForceComponent>(world);
                component.Time = world.GetTime() + 1f;
                var angle = Math.PI / 8f;
                var rotated = new Vector3();
                rotated.x = (float)(dir.x * Math.Cos(angle) - dir.z * Math.Sin(angle));
                rotated.z = (float)(dir.x * Math.Sin(angle) + dir.z * Math.Cos(angle));
                component.Direction = rotated;
            }
        }
        
        private void Shoot(int unitEntity, InputShotComponent shoot)
        {
            if (unitEntity.EntityHas<ShootingComponent>(world) &&
                !unitEntity.EntityGet<ShootingComponent>(world).ShootMade)
            {
                //Debug.Log("skip");
                return;
            }
            
            if (unitEntity.EntityHas<AmmoCollectedComponent>(world) &&
                unitEntity.EntityGet<AmmoCollectedComponent>(world).Value <= 0)
            {
                return;
            }

            world.Log($"input shot");

            //unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            unitEntity.EntityAdd<ShootStartedComponent>(world);
            unitEntity.EntityReplace(world, new ShootingComponent
            {
                Direction = shoot.dir,
                Position = shoot.pos,
                ShootAtTime = world.GetTime() + 0.2f, 
                TotalTime = world.GetTime() + 0.5f
            });
        }


        private int GetControlledEntity(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                int mechEntity;
                if (unitEntity.EntityGet<ControlsMechComponent>(world).PackedEntity.Unpack(world, out mechEntity))
                    return mechEntity;
            }

            return unitEntity;
        }
        
        private void Move(int unitEntity, Vector3 dir)
        {
            var entity = GetControlledEntity(unitEntity);

            if (entity.EntityHas<CantMoveComponent>(world))
                return;
                    
            if (dir.sqrMagnitude > 0.001f)
            {
                entity.EntityDel<TargetPositionComponent>(world);
                entity.EntityGetOrCreateRef<MoveDirectionComponent>(world).value = dir;
            }
            else
            {
                if (entity.EntityHas<MoveDirectionComponent>(world))
                {
                    entity.EntityDel<MoveDirectionComponent>(world);
                    entity.EntityDel<MovingComponent>(world);
                }
            }
        }

        private void MoveToPoint(int unitEntity, Vector3 pos)
        {
            var entity = GetControlledEntity(unitEntity);
            
            ref var targetPositionComponent = ref entity.EntityGetOrCreateRef<TargetPositionComponent>(world);
            targetPositionComponent.Value = pos;
            
        }
        
        private void MoveItem(MoveItemComponent data)
        {
            if (!data.Inventory.Unpack(world, out var inventory))
            {
                return;
            }
            
            if (!data.Item.Unpack(world, out var item))
            {
                return;
            }

            if (data.Amount == -1)
            {
                inventoryService.Add(world, inventory, item);
                return;
            }

            inventoryService.Add(world, inventory, item, data.Amount);
        }

        private void ClearInventory(ClearInventoryComponent data)
        {
            if (!data.Inventory.Unpack(world, out var inventory))
            {
                return;
            }

            inventoryService.Clear(world, inventory);
        }
    }
}
