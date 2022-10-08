using System;
using System.Collections.Generic;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Game.ClientServer;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Inventory;
using UnityEngine;
using XFlow.Ecs.ClientServer;
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
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsWorld _inputWorld;

        private MyInventoryService _inventoryService;
        
        private List<int> _entities = new List<int>();

        public ApplyInputSystem(MyInventoryService myInventoryService)
        {
            _inventoryService = myInventoryService;
        }
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<InputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var mainPlayerId = -1;
            if (_world.HasUnique<MainPlayerIdComponent>())//если это мир на клиенте
                mainPlayerId = _world.GetUnique<MainPlayerIdComponent>().value;
           
            
            var poolInputShot   = _inputWorld.GetPool<InputShotComponent>();
            var poolPlayer      = _inputWorld.GetPool<InputPlayerComponent>();
            var poolInputMoveDir= _inputWorld.GetPool<InputMoveDirectionComponent>();
            var poolInputMoveTo = _inputWorld.GetPool<InputMoveToPointComponent>();
            var poolMoveItem = _inputWorld.GetPool<MoveItemComponent>();
            var poolClearInventory = _inputWorld.GetPool<ClearInventoryComponent>();
            var poolInputAction = _inputWorld.GetPool<InputActionComponent>();
            var poolInputKick   = _inputWorld.GetPool<InputKickComponent>();
            var poolInputTick   = _inputWorld.GetPool<InputTickComponent>();
            
            
            

            var tick = _world.GetTick();
            
            foreach (var inputEntity in _filter)
            {
                if (poolInputTick.GetNullable(inputEntity)?.Tick != tick)
                    continue;
                
                var playerId = mainPlayerId;
                if (poolPlayer.Has(inputEntity))
                    playerId = poolPlayer.Get(inputEntity).PlayerID;

                var unitEntity = BaseServices.GetUnitEntityByPlayerId(_world, playerId);
                if (!_world.IsEntityAliveInternal(unitEntity))
                {
                    _world.LogError($"unit entity {unitEntity} is not alive");
                    continue;
                }

                if (!unitEntity.EntityHas<UnitComponent>(_world))
                {
                    _world.LogError($"entity {unitEntity} is not unit");
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
                    Interract(_world, unitEntity);
                }
                
                if (poolInputKick.Has(inputEntity))
                {
                    var dir = poolInputKick.Get(inputEntity).dir;
                    Kick(unitEntity, dir);
                }

                if (inputEntity.EntityHas<InputMechEnterLeaveComponent>(_inputWorld))
                {
                    EnterLeaveMech(unitEntity);
                }

                if (inputEntity.EntityHas<MoveItemComponent>(_inputWorld))
                {
                    MoveItem(poolMoveItem.Get(inputEntity));
                }

                if (inputEntity.EntityHas<ClearInventoryComponent>(_inputWorld))
                {
                    ClearInventory(poolClearInventory.Get(inputEntity));
                }
            }
        }

        public void EnterLeaveMech(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(_world))
            {
                unitEntity.EntityDel<ControlsMechComponent>(_world);
                return;
            }
            
            _world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(_world).Value,
                1, ref _entities, entity=> entity.EntityHas<MechComponent>(_world));

            if (_entities.Count == 0)
                return;
            
            var entity = _entities[0];
            ref var packedEntity = ref unitEntity.EntityAdd<ControlsMechComponent>(_world).PackedEntity;
            packedEntity = _world.PackEntity(entity);
        }
        
        public void Interract(EcsWorld world, int unitEntity)
        {
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).Value,
                1, ref _entities, entity=> entity.EntityHas<InteractableComponent>(world));

            if (_entities.Count == 0)
                return;
            
            var entity = _entities[0];
            if (entity.EntityHas<SpawnGunComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<WeaponComponent>(world);
                //TryHideCollected(entity);
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

                _inventoryService.Add(world, inventory, berryEntity);
            }

            if (entity.EntityHas<AmmoComponent>(world))
            {
                unitEntity.EntityGetOrCreateRef<AmmoCollectedComponent>(world).Value += 10;
                //entity.EntityDel<InteractableComponent>(world);
                //TryHideCollected(entity);
            }
        }

        void TryHideCollected(int entity)
        {
            if (entity.EntityHas<CollectableComponent>(_world))
            {
                entity.EntityGetRef<CollectableComponent>(_world).isCollected = true;
            }
        }
        
        public void Kick(int unitEntity, Vector3 dir)
        {
            if (unitEntity.EntityHas<PushingComponent>(_world))
                return;
                
            unitEntity.EntityAdd<PushingComponent>(_world).EndTime = _world.GetTime() + 1.3f;
            unitEntity.EntityGetOrCreateRef<CantMoveComponent>(_world);
            
            if (unitEntity.EntityHas<LookDirectionComponent>(_world) && 
                !unitEntity.EntityHas<ApplyForceComponent>(_world))
            {
                ref var component = ref unitEntity.EntityAdd<ApplyForceComponent>(_world);
                component.Time = _world.GetTime() + 1f;
                var angle = Math.PI / 8f;
                var rotated = new Vector3();
                rotated.x = (float)(dir.x * Math.Cos(angle) - dir.z * Math.Sin(angle));
                rotated.z = (float)(dir.x * Math.Sin(angle) + dir.z * Math.Cos(angle));
                component.Direction = rotated;
            }
        }
        
        private void Shoot(int unitEntity, InputShotComponent shoot)
        {
            if (unitEntity.EntityHas<ShootingComponent>(_world) &&
                !unitEntity.EntityGet<ShootingComponent>(_world).ShootMade)
            {
                //Debug.Log("skip");
                return;
            }
            
            if (unitEntity.EntityHas<AmmoCollectedComponent>(_world) &&
                unitEntity.EntityGet<AmmoCollectedComponent>(_world).Value <= 0)
            {
                return;
            }

            _world.LogVerbose($"input shot");

            //unitEntity.EntityGetOrCreateRef<CantMoveComponent>(world);
            unitEntity.EntityAdd<ShootStartedComponent>(_world);
            unitEntity.EntityReplace(_world, new ShootingComponent
            {
                Direction = shoot.dir,
                Position = shoot.pos,
                ShootAtTime = _world.GetTime() + 0.2f, 
                TotalTime = _world.GetTime() + 0.5f
            });
        }


        private int GetControlledEntity(int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(_world))
            {
                int mechEntity;
                if (unitEntity.EntityGet<ControlsMechComponent>(_world).PackedEntity.Unpack(_world, out mechEntity))
                    return mechEntity;
            }

            return unitEntity;
        }
        
        private void Move(int unitEntity, Vector3 dir)
        {
            var entity = GetControlledEntity(unitEntity);

            if (entity.EntityHas<CantMoveComponent>(_world))
                return;
                    
            if (dir.sqrMagnitude > 0.001f)
            {
                entity.EntityDel<TargetPositionComponent>(_world);
                entity.EntityGetOrCreateRef<MoveDirectionComponent>(_world).Value = dir;
            }
            else
            {
                if (entity.EntityHas<MoveDirectionComponent>(_world))
                {
                    entity.EntityDel<MoveDirectionComponent>(_world);
                    entity.EntityDel<MovingComponent>(_world);
                }
            }
        }

        private void MoveToPoint(int unitEntity, Vector3 pos)
        {
            var entity = GetControlledEntity(unitEntity);
            
            ref var targetPositionComponent = ref entity.EntityGetOrCreateRef<TargetPositionComponent>(_world);
            targetPositionComponent.Value = pos;
            
        }
        
        private void MoveItem(MoveItemComponent data)
        {
            if (!data.Inventory.Unpack(_world, out var inventory))
            {
                return;
            }
            
            if (!data.Item.Unpack(_world, out var item))
            {
                return;
            }

            if (data.Amount == -1)
            {
                _inventoryService.Add(_world, inventory, item);
                return;
            }

            _inventoryService.Add(_world, inventory, item, data.Amount);
        }

        private void ClearInventory(ClearInventoryComponent data)
        {
            if (!data.Inventory.Unpack(_world, out var inventory))
            {
                return;
            }

            _inventoryService.Clear(_world, inventory);
        }
    }
}
