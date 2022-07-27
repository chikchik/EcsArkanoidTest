using Fabros.Ecs.Client.Components;
using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;
using Flow.EcsLite;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.Client.Systems
{
    public class WeaponEquipSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .IncAdded<WeaponComponent>()
                .End();
            var poolCollectable = world.GetPool<CollectableComponent>();
            var poolCollectableTarget = world.GetPool<CollectableTargetComponent>();

            foreach (var entity in filter)
            {
                var transform = entity.EntityGet<TransformComponent>(world).Transform;
                var view = transform.GetComponent<CharacterView>();
                view.Gun.gameObject.SetActive(true);
            }
        }
    }
}