using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.Client.Systems
{
    public class DetectPlayerIdChangesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.TryGetUnique<MainPlayerIdComponent>(out var comp))
                return;
            int mainPlayerID = comp.value;

            var filter = world
                .Filter<PlayerComponent>().IncChanges<PlayerComponent>()
                .End();

            foreach (var entity in filter)
            {
                if (entity.EntityGet<PlayerComponent>(world).id != mainPlayerID)
                    continue;

                ClientPlayerService.SetPlayerEntity(world, entity);
                break;
            }
        }
    }
}