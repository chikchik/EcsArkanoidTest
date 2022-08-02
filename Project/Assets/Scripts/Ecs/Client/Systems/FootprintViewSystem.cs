using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;

using Game.View;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;
using Zenject;

namespace Game.Ecs.Client.Systems
{
    public class FootprintViewSystem : IEcsRunSystem
    {
        private FootprintView leftPrefab;
        private FootprintView rightPrefab;
        
        public FootprintViewSystem([Inject(Id="left")]FootprintView leftPrefab, [Inject(Id="right")]FootprintView rightPrefab)
        {
            this.leftPrefab = leftPrefab;
            this.rightPrefab = rightPrefab;
        }
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var filter = world
                .Filter<FootprintComponent>()
                .Exc<TransformComponent>()
                .Exc<DestroyComponent>()
                .End();
            var poolFootprint = world.GetPool<FootprintComponent>();

            foreach (var entity in filter)
            {
                var footprintComponent = poolFootprint.Get(entity);
                CreateView(world, entity, footprintComponent);
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

        private void CreateView(EcsWorld world, int entity, FootprintComponent footprintComponent)
        {
            var footprintPrefab = footprintComponent.isLeftHand ? leftPrefab : rightPrefab;

            var view = GameObject.Instantiate(footprintPrefab);
            var transform = view.transform;
            
            ref var transformComponent = ref entity.EntityAddComponent<TransformComponent>(world);
            transformComponent.Transform = transform;

            transform.name = $"footprint {footprintComponent.isLeftHand} - {entity}";
            transform.position = entity.EntityGetComponent<PositionComponent>(world).value;
            if (footprintComponent.direction.magnitude > 0.9f)
                transform.forward = footprintComponent.direction;
        }
    }
}