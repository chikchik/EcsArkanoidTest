using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.Client.Systems
{
    public class CreateViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private CharacterView _viewPrefab;
        private BulletView _bulletPrefab;
        public CreateViewSystem(CharacterView viewPrefab, BulletView bulletPrefab)
        {
            this._bulletPrefab = bulletPrefab;
            this._viewPrefab = viewPrefab;
        }
        
        private EcsWorld world;
        private EcsFilter filter;
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<GameObjectNameComponent>()
                .Exc<TransformComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {

            foreach (var entity in filter)
            {
                var name = entity.EntityGet<GameObjectNameComponent>(world).Name.ToString();
                var go = GameObject.Find(name);
                if (go == null)
                {
                    Debug.LogError($"not found gameobject {name}");
                }
                go.SetActive(true);


                entity.EntityAdd<TransformComponent>(world).Transform = go.transform;

                if (entity.EntityHas<CollectableComponent>(world))
                {
                    if (entity.EntityHas<BushComponent>(world))
                    {
                        var view = go.GetComponent<BushView>();
                        ref var collectableTargetComponent =
                            ref entity.EntityAdd<CollectableTargetComponent>(world);
                        collectableTargetComponent.targetObject = view.Berries.gameObject;
                    }
                    else
                    {
                        ref var collectableTargetComponent =
                            ref entity.EntityAdd<CollectableTargetComponent>(world);
                        collectableTargetComponent.targetObject = go;
                    }
                }
            }

            var filterUnits = world.Filter<UnitComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterUnits)
            {
                var view = Object.Instantiate(_viewPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(world).value;
                view.Gun.gameObject.SetActive(false);

                ref var component = ref entity.EntityAdd<TransformComponent>(world);
                component.Transform = view.transform;
                
                ref var animatorComponent = ref entity.EntityAdd<AnimatorComponent>(world);
                animatorComponent.animator = view.Animator;

                entity.EntityGetOrCreateRef<LerpComponent>(world).value = 0.5f;
            }
            
            var filterBullets = world.Filter<BulletComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterBullets)
            {
                var view = Object.Instantiate(_bulletPrefab);
                var pos = entity.EntityGet<PositionComponent>(world).value;
                view.transform.position = pos;
                view.name = $"Bullet{entity}";

                ref var component = ref entity.EntityAdd<TransformComponent>(world);
                component.Transform = view.transform;

                entity.EntityGetOrCreateRef<LerpComponent>(world).value = 0.5f;
                
                world.LogVerbose($"create view {entity} at {pos}");
            }
        }
    }
}