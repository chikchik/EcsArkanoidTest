using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.View;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Input;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class PlayerControlService : IInventoryInputService
    {
        private EcsWorld _inputWorld;
        private EcsWorld _world;

        private IInputService _input;
        
        public PlayerControlService(
            [Inject(Id = EcsWorlds.Input)] EcsWorld inputWorld,
            [InjectOptional] IInputService input,
            EcsWorld world)
        {
            this._inputWorld = inputWorld;
            this._world = world;
            this._input = input;
        }
        
        private int playerId => _world.GetUnique<MainPlayerIdComponent>().value;
        private int unitEntity => BaseServices.GetUnitEntityByPlayerId(_world, playerId);
        private int tick => _world.GetTick();
        
        public void Shot()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            var view = unitEntity.EntityGet<TransformComponent>(_world).Transform.GetComponent<CharacterView>();
            //view.
            
            var component = new InputShotComponent();
            var lookDir = _world.EntityGet<LookDirectionComponent>(unitEntity).value;
            var dir = Quaternion.Euler(0, -0, 0) * lookDir;
            component.dir = dir;
            component.pos = view.BulletSpawnPos.transform.position; 
            
            Apply(component);
        }
        
        public void Interact()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputActionComponent();
            Apply(component);
        }
        
        public void Kick()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputKickComponent();
            component.dir = _world.EntityGet<LookDirectionComponent>(unitEntity).value;
            
            Apply(component);
        }
        
        public void MechEnterLeave()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            Apply(new InputMechEnterLeaveComponent());
        }

        public void MoveItem(int inventory, int item, int amount)
        {
            var component = new MoveItemComponent
            {
                Inventory = _world.PackEntity(inventory),
                Item = _world.PackEntity(item),
                Amount = amount
            };
            
            Apply(component);
        }

        public void ClearInventory(int inventory)
        {
            var component = new ClearInventoryComponent
            {
                Inventory = _world.PackEntity(inventory)
            };

            Apply(component);
        }
        
        public void MoveToDirection(Vector3 dir)
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }
            
            var component = new InputMoveDirectionComponent();
            component.Dir = dir;
            
            Apply(component);
        }
        
        public void StopMoveToDirection()
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            var entity = ApplyInputWorldService.GetControlledEntity(_world, unitEntity);
            
            if (!entity.EntityHas<MoveDirectionComponent>(_world))
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Dir = Vector3.zero;
            
            Apply(component);
        }
        
        public void MoveToPoint(Vector3 pos)
        {
            if (unitEntity == -1)
            {
                Debug.LogWarning("unitEntity == -1");
                return;
            }

            var component = new InputMoveToPointComponent();
            component.Value = pos;
            Apply(component);
        }

        public void BeginDrag(int entity)
        {
            
        }

        public void UpdateDrag(Vector3 pos)
        {
            
        }
        
        public void EndDrag()
        {
            
        }

        private void Apply(IInputComponent component)
        {
            //reused on server
            ApplyInputWorldService.CreateInputEntity(_inputWorld, playerId, tick, component);
            
            //send player input to server
            _input?.Input(_inputWorld, playerId, tick, component);
        }
    }
}