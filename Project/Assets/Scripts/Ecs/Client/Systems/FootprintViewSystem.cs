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
        private FootprintView _leftPrefab;
        private FootprintView _rightPrefab;
        
        public FootprintViewSystem([Inject(Id="left")]FootprintView leftPrefab, [Inject(Id="right")]FootprintView rightPrefab)
        {
            this._leftPrefab = leftPrefab;
            this._rightPrefab = rightPrefab;
        }
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var filter = world
                .Filter<FootprintComponent>()
                .Exc<TransformComponent>()
                .End();
            var poolFootprint = world.GetPool<FootprintComponent>();

            foreach (var entity in filter)
            {
                var footprintComponent = poolFootprint.Get(entity);
                CreateView(world, entity, footprintComponent);
            }

            /*
            var destroyEntities = world
                .Filter<FootprintComponent>()
                .Inc<TransformComponent>()
                .Inc<DestroyComponent>()
                .End();

            foreach (var entity in destroyEntities)
            {
                var transform = entity.EntityGetRef<TransformComponent>(world).Transform;
                Object.Destroy(transform.gameObject);
            }*/
        }

        private void CreateView(EcsWorld world, int entity, FootprintComponent footprintComponent)
        {
            var footprintPrefab = footprintComponent.isLeftHand ? _leftPrefab : _rightPrefab;

            var view = GameObject.Instantiate(footprintPrefab);
            var transform = view.transform;
            
            ref var transformComponent = ref entity.EntityAdd<TransformComponent>(world);
            transformComponent.Transform = transform;

            transform.name = $"footprint {footprintComponent.isLeftHand} - {entity}";
            transform.position = entity.EntityGet<PositionComponent>(world).Value;
            if (footprintComponent.direction.magnitude > 0.9f)
                transform.forward = footprintComponent.direction;
        }
    }
}