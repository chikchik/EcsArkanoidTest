using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class FootprintViewSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var filter = world
                .Filter<FootprintComponent>()
                .Exc<TransformComponent>()
                .Exc<DestroyComponent>()
                .End();
            var poolFootprint = world.GetPool<FootprintComponent>();
            var global = world.GetUnique<ClientViewComponent>().Global;

            foreach (var entity in filter)
            {
                var footprintComponent = poolFootprint.Get(entity);
                CreateView(world, entity, global, footprintComponent);
            }

            var destroyEntities = world
                .Filter<FootprintComponent>()
                .Inc<TransformComponent>()
                .Inc<DestroyComponent>()
                .End();

            foreach (var entity in destroyEntities)
            {
                var transform = entity.EntityGetRefComponent<TransformComponent>(world).Transform;
                Object.Destroy(transform.gameObject);
            }
        }

        private void CreateView(EcsWorld world, int entity, Global global, FootprintComponent footprintComponent)
        {
            var footprintPrefab =
                footprintComponent.isLeftHand ? global.leftFootprintPrefab : global.rightFootprintPrefab;

            var transform = Object.Instantiate(footprintPrefab, global.footprintParent).transform;
            
            ref var transformComponent = ref entity.EntityAddComponent<TransformComponent>(world);
            transformComponent.Transform = transform;

            transform.name = $"footprint {footprintComponent.isLeftHand} - {entity}";
            transform.position = entity.EntityGetComponent<PositionComponent>(world).value;
            if (footprintComponent.direction.magnitude > 0.9f)
                transform.forward = footprintComponent.direction;
        }
    }
}