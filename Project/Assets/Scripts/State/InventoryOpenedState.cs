using Game.Ecs.ClientServer.Components.Inventory;
using Unity.VisualScripting.IonicZip;
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
        private InventoryFactory inventoryFactory;
        private UIInventory inventoryView;
        private UI.UI uiView;
        private EcsWorld world;
        
        public InventoryOpenedState(States states,
            EcsWorld world,
            InventoryFactory inventoryFactory, 
            PlayerControlService playerControlService,
            UI.UI uiView
            ) : base(states)
        {
            this.inventoryFactory = inventoryFactory;
            this.uiView = uiView;
            this.world = world;
        }

        public override void Enter()
        {
            base.Enter();

            inventoryView = inventoryFactory.CreateInventory(uiView.View.transform);
            inventoryView.GetCloseButton().onClick.AddListener(() =>
            {
                Close();
            });

            var playerId = world.GetUnique<MainPlayerIdComponent>().value;
            var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerId);

            var poolInventoryLink = world.GetPool<InventoryLinkComponent>();
            if (!poolInventoryLink.Has(unitEntity))
            {
                return;
            }

            if (!poolInventoryLink.Get(unitEntity).Inventory.Unpack(world, out var inventoryEntity))
            {
                return;
            }

            var poolTrashLink = world.GetPool<TrashLinkComponent>();
            if (!poolTrashLink.Has(unitEntity))
            {
                return;
            }

            if (!poolTrashLink.Get(unitEntity).Trash.Unpack(world, out var trashEntity))
            {
                return;
            }

            inventoryView.Init(inventoryEntity, trashEntity, world);
        }

        public override void Exit()
        {
            base.Exit();
            GameObject.Destroy(inventoryView.gameObject);
            inventoryView = null;
        }
    }
}