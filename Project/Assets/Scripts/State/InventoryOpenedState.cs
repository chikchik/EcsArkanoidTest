using Game.Ecs.ClientServer.Components.Inventory;
using Game.UI;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.Client.Demo.Services;
using XFlow.Modules.Inventory.Client.Demo.UI;
using XFlow.Modules.States;
using XFlow.Modules.States.FSM;
using XFlow.Net.ClientServer;

namespace Game.State
{
    public class InventoryOpenedState : StateWithUI
    {
        private InventoryFactory _inventoryFactory;
        private UIInventory _inventoryView;
        private UI.UI _uiView;
        private EcsWorld _world;
        
        public InventoryOpenedState(States states,
            EcsWorld world,
            InventoryFactory inventoryFactory, 
            PlayerControlService playerControlService,
            UI.UI uiView
            ) : base(states)
        {
            this._inventoryFactory = inventoryFactory;
            this._uiView = uiView;
            this._world = world;
        }

        public override void Enter()
        {
            base.Enter();

            _inventoryView = _inventoryFactory.CreateInventory(_uiView.View.transform);
            _inventoryView.GetCloseButton().onClick.AddListener(() =>
            {
                Close();
            });

            if (!ClientPlayerService.TryGetControlledEntity(_world, out int unitEntity))
                return;
            
            var poolInventoryLink = _world.GetPool<InventoryLinkComponent>();
            if (!poolInventoryLink.Has(unitEntity))
            {
                return;
            }

            if (!poolInventoryLink.Get(unitEntity).Inventory.Unpack(_world, out var inventoryEntity))
            {
                return;
            }

            var poolTrashLink = _world.GetPool<TrashLinkComponent>();
            if (!poolTrashLink.Has(unitEntity))
            {
                return;
            }

            if (!poolTrashLink.Get(unitEntity).Trash.Unpack(_world, out var trashEntity))
            {
                return;
            }

            _inventoryView.Init(inventoryEntity, trashEntity, _world);
        }

        public override void Exit()
        {
            base.Exit();
            GameObject.Destroy(_inventoryView.gameObject);
            _inventoryView = null;
        }
    }
}