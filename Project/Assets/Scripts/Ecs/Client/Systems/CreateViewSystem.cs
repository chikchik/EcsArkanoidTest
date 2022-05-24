using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
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
                .Exc<GameObjectComponent>().End();

            foreach (var entity in filter)
            {
                var name = entity.EntityGetComponent<GameObjectNameComponent>(world).Name;
                var go = GameObject.Find(name).gameObject;
                go.gameObject.SetActive(true);

                entity.EntityAddComponent<GameObjectComponent>(world).GameObject = go;
                entity.EntityAddComponent<TransformComponent>(world).transform = go.transform;

                if (entity.EntityHasComponent<CollectableComponent>(world))
                {
                    var bushView = go.GetComponent<BushView>();
                    ref var collectableTargetComponent = ref entity.EntityAddComponent<CollectableTargetComponent>(world);
                    collectableTargetComponent.targetObject = bushView.Berries.gameObject;
                }
            }

            var filterUnits = world.Filter<UnitComponent>()
                .Exc<GameObjectComponent>().End();

            foreach (var entity in filterUnits)
            {
                Debug.Log($"link {entity}");

                var characterView = Object.Instantiate(viewComponent.Global.characterPrefab);

                ref var component = ref entity.EntityAddComponent<GameObjectComponent>(world);
                component.GameObject = characterView.gameObject;
                
                ref var transformComponent = ref entity.EntityAddComponent<TransformComponent>(world);
                transformComponent.transform = characterView.transform;

                ref var animatorComponent = ref entity.EntityAddComponent<AnimatorComponent>(world);
                animatorComponent.animator = characterView.Animator;

                var position = entity.EntityGet<PositionComponent>(world).value;
                entity.EntityAdd<RootMotionComponent>(world).Position = position;

                transformComponent.transform.position = position;

                entity.EntityReplaceComponent<LerpComponent>(world).value = 0.5f;
            }
        }
    }
}