using DG.Tweening;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.Client.Systems
{
    public class BulletDestroyFxSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsPool<ReliableComponent> _poolReliable;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.FilterMarkedDeleted().Inc<BulletComponent>().Inc<TransformComponent>().End();
            _poolReliable = _world.GetPool<ReliableComponent>();
        }

        public void Run(EcsSystems systems)
        {
            return;
            foreach (var entity in _filter)
            {
                var transform = entity.EntityGet<TransformComponent>(_world).Transform;
                entity.EntityDel<TransformComponent>(_world);
                if (!_poolReliable.Has(entity))
                {
                    GameObject.Destroy(transform.gameObject);
                    continue;
                }

                transform.position = entity.EntityGet<PositionComponent>(_world).value;
                transform.DOScale(0.5f, 0.5f).OnComplete(() =>
                {
                    GameObject.Destroy(transform.gameObject);  
                });
                
                
            }
        }
    }
}