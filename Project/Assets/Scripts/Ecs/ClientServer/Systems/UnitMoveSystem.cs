using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;

namespace Game.Ecs.ClientServer.Systems
{
    public class UnitMoveSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsPool<Box2DLinearVelocityComponent> _poolLinearVelocity;
        private EcsPool<MovingComponent> _movingPool;
        private EcsPool<MoveDirectionComponent> _moveDirectionPool;
        private EcsPool<AverageSpeedComponent> _speedPool;
        private EcsPool<CantMoveComponent> _cantMovePool;

        private EcsFilter _moveFilter;
        private EcsFilter _stopFilter;

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _moveFilter)
            {
                if (_cantMovePool.Has(entity))
                {
                    _poolLinearVelocity.GetRef(entity).Value = Vector2.zero;
                    continue;
                }

                var moveDirectionComponent = _moveDirectionPool.Get(entity);
                var speedComponent = _speedPool.Get(entity);
                
                _poolLinearVelocity.GetRef(entity).Value =
                    new Vector2(moveDirectionComponent.Value.x, moveDirectionComponent.Value.z).normalized *
                    speedComponent.Value;
                
                _movingPool.Replace(entity, new MovingComponent());
            }

            foreach (var entity in _stopFilter)
            {
                ref var rbComponent = ref _poolLinearVelocity.GetRef(entity);
                rbComponent.Value = Vector2.zero;
            }
        }


        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            _poolLinearVelocity = world.GetPool<Box2DLinearVelocityComponent>();
            _movingPool = world.GetPool<MovingComponent>();
            _moveDirectionPool = world.GetPool<MoveDirectionComponent>();
            _speedPool = world.GetPool<AverageSpeedComponent>();
            _cantMovePool = world.GetPool<CantMoveComponent>();

            _moveFilter = world
                .Filter<MoveDirectionComponent>()
                .Inc<AverageSpeedComponent>()
                .Inc<UnitComponent>()
                .Inc<Box2DBodyComponent>()
                .End();

            _stopFilter = world
                .Filter<UnitComponent>()
                .Exc<MoveDirectionComponent>()
                .Exc<TargetPositionComponent>()
                .Inc<Box2DBodyComponent>()
                .End();

        }
    }
}