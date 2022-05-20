using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
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
                .Exc<GameObjectComponent>()
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
                .Inc<GameObjectComponent>()
                .Inc<DestroyComponent>()
                .End();

            foreach (var entity in destroyEntities)
            {
                var gameObjectComponent = entity.EntityGetRefComponent<GameObjectComponent>(world);
                Object.Destroy(gameObjectComponent.GameObject);
            }
        }

        private void CreateView(EcsWorld world, int entity, Global global, FootprintComponent footprintComponent)
        {
            var footprintPrefab =
                footprintComponent.isLeftHand ? global.leftFootprintPrefab : global.rightFootprintPrefab;

            ref var gameObjectComponent = ref entity.EntityAddComponent<GameObjectComponent>(world);
            gameObjectComponent.GameObject = Object.Instantiate(footprintPrefab, global.footprintParent);
            gameObjectComponent.GameObject.name = $"footprint {footprintComponent.isLeftHand} - {entity}";

            ref var transformComponent = ref entity.EntityAddComponent<TransformComponent>(world);
            transformComponent.transform = gameObjectComponent.GameObject.transform;
            transformComponent.transform.position = entity.EntityGetComponent<PositionComponent>(world).value;
            if (footprintComponent.direction.magnitude > 0.9f)
                transformComponent.transform.forward = footprintComponent.direction;
        }
    }
}