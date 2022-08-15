using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;

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

            if (!world.HasUnique<MainPlayerIdComponent>())
                return;

            var filter = world
                .Filter<PlayerComponent>().IncChanges<PlayerComponent>()
                .End();

            var mainPlayerID = world.GetUnique<MainPlayerIdComponent>().value;
            foreach (var entity in filter)
            {
                if (entity.EntityGet<PlayerComponent>(world).id == mainPlayerID)
                {
                    //replace потому что ClientPlayerComponent мог уже быть 
                    world.GetOrCreateUniqueRef<ClientPlayerComponent>().entity = entity;
                    entity.EntityGetOrCreateRef<LerpComponent>(world).value = 1;
                    entity.EntityAdd<IsMainPlayerComponent>(world);
                }
            }
        }
    }
}