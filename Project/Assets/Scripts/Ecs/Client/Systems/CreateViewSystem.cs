using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer;
using Game.View;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class CreateViewSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var viewComponent = world.GetUnique<ClientViewComponent>();

            var filter = world.Filter<GameObjectNameComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filter)
            {
                var name = entity.EntityGetComponent<GameObjectNameComponent>(world).Id.ToString();
                var go = GameObject.Find(name).gameObject;
                go.gameObject.SetActive(true);


                entity.EntityAddComponent<TransformComponent>(world).Transform = go.transform;

                if (entity.EntityHasComponent<CollectableComponent>(world))
                {
                    if (entity.EntityHas<BushComponent>(world))
                    {
                        var view = go.GetComponent<BushView>();
                        ref var collectableTargetComponent =
                            ref entity.EntityAddComponent<CollectableTargetComponent>(world);
                        collectableTargetComponent.targetObject = view.Berries.gameObject;
                    }
                    else
                    {
                        var view = go.GetComponent<SpawnGunView>();
                        ref var collectableTargetComponent =
                            ref entity.EntityAddComponent<CollectableTargetComponent>(world);
                        collectableTargetComponent.targetObject = view.gameObject;
                    }
                }
            }

            var filterUnits = world.Filter<UnitComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterUnits)
            {
                var view = Object.Instantiate(viewComponent.Global.characterPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(world).value;
                view.Gun.gameObject.SetActive(false);

                ref var component = ref entity.EntityAddComponent<TransformComponent>(world);
                component.Transform = view.transform;
                
                ref var animatorComponent = ref entity.EntityAddComponent<AnimatorComponent>(world);
                animatorComponent.animator = view.Animator;

                entity.EntityGetOrCreateRef<LerpComponent>(world).value = 0.5f;
            }
            
            var filterBullets = world.Filter<BulletComponent>()
                .Exc<TransformComponent>().End();

            foreach (var entity in filterBullets)
            {
                var view = Object.Instantiate(viewComponent.Global.BulletPrefab);
                var pos = entity.EntityGet<PositionComponent>(world).value;
                view.transform.position = pos;
                view.name = $"Bullet{entity}";

                ref var component = ref entity.EntityAddComponent<TransformComponent>(world);
                component.Transform = view.transform;

                entity.EntityGetOrCreateRef<LerpComponent>(world).value = 0.5f;
                
                world.Log($"create view {entity} at {pos}");
            }
        }
    }
}