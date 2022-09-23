using DG.Tweening;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.Client.Systems
{
    public class DestroyViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsPool<ReliableComponent> _poolReliable;
        private EcsPool<TransformComponent> _poolTransform;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.FilterBase().Inc<DestroyedEntityComponent>().Inc<TransformComponent>().End();
            _poolTransform = _world.GetPool<TransformComponent>();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var transform = _poolTransform.Get(entity).Transform;
                GameObject.Destroy(transform.gameObject);
                entity.EntityDel<TransformComponent>(_world);
                
                _world.Log($"destroy view {entity.e2name(_world)} go: {transform.name}");
                /*
                
                if (!_poolReliable.Has(entity))
                {
                    GameObject.Destroy(transform.gameObject);
                    continue;
                }

                transform.position = entity.EntityGet<PositionComponent>(_deadWorld).value;
                transform.DOScale(0.5f, 0.5f).OnComplete(() =>
                {
                    GameObject.Destroy(transform.gameObject);  
                });
                */
            }
        }
    }
}