using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Inventory;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class JoinPlayerSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputJoinPlayerComponent>().End();

            foreach (var inputEntity in filter)
            {
                var joinPlayerComponent = inputEntity.EntityGetComponent<InputJoinPlayerComponent>(inputWorld);
                var playerID = joinPlayerComponent.playerID;
                var leave = joinPlayerComponent.leave;

                var ms = leave ? "leave" : "join";
                //context.WriteToConsole?.Invoke($"{ms} player {playerID}");
                if (leave)
                {
                    var unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerID);
                    if (unitEntity != -1)
                    {
                        world.DelEntity(unitEntity);
                        //unitEntity.EntityWithRef(world, (ref PlayerComponent data) => { data.id = -1; });
                    }
                }
                else
                {
                    var freeUnitEntity = UnitService.CreateUnitEntity(world);
                    freeUnitEntity.EntityGetOrCreateRef<PlayerComponent>(world).id = playerID;
                    
                    var inventory = world.NewEntity();
                    inventory.EntityAddComponent<InventoryComponent>(world).SlotCapacity = 10;

                    var trash = world.NewEntity();
                    trash.EntityAddComponent<InventoryComponent>(world).SlotCapacity = 10;

                    freeUnitEntity.EntityAddComponent<InventoryLinkComponent>(world).Inventory =
                        world.PackEntity(inventory);
                    freeUnitEntity.EntityAddComponent<TrashLinkComponent>(world).Trash = world.PackEntity(trash);
                }
            }
        }
    }
}