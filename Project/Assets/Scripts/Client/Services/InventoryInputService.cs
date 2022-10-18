using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;

namespace Game.Client.Services
{
    public class InventoryInputService : IInventoryInputService
    {
        private PlayerControlService _inputService;
        private EcsWorld _mainWorld;
        
        public InventoryInputService(
            PlayerControlService inputService,
            EcsWorld mainWorld)
        {
            _inputService = inputService;
            _mainWorld = mainWorld;
        }
        
        public void MoveItem(int inventory, int item, int amount)
        {
            var component = new MoveItemComponent
            {
                Inventory = _mainWorld.PackEntity(inventory),
                Item = _mainWorld.PackEntity(item),
                Amount = amount
            };
            
            _inputService.Apply(component);
        }

        public void ClearInventory(int inventory)
        {
            var component = new ClearInventoryComponent
            {
                Inventory = _mainWorld.PackEntity(inventory)
            };

            _inputService.Apply(component);
        }
    }
}