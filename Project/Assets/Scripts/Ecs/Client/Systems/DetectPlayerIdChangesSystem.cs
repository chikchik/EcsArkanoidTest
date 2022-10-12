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

            if (!world.HasUnique<MainPlayerIdComponent>())
                return;

            var filter = world
                .Filter<PlayerComponent>().IncChanges<PlayerComponent>()
                .End();

            var mainPlayerID = world.GetUnique<MainPlayerIdComponent>().Value;
            foreach (var entity in filter)
            {
                if (entity.EntityGet<PlayerComponent>(world).Value == mainPlayerID)
                {
                    ClientPlayerService.SetPlayerEntity(world, entity);
                    int i = 0;
                    /*
                    //replace потому что ClientPlayerComponent мог уже быть 
                    world.GetOrCreateUniqueRef<ClientPlayerEntityComponent>().Player = entity;
                    world.GetOrCreateUniqueRef<ClientPlayerEntityComponent>().Unit = entity;
                    
                    entity.EntityGetOrCreateRef<LerpComponent>(world).Value = 1;
                    entity.EntityAdd<IsMainPlayerComponent>(world);*/
                }
            }
        }
    }
}