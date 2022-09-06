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
        private EcsPool<Box2DRigidbodyComponent> _rbPool;
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
                    _rbPool.GetRef(entity).LinearVelocity = Vector2.zero;
                    continue;
                }

                SetLinearVelToBody(entity);
                _movingPool.Replace(entity, new MovingComponent());
            }

            foreach (var entity in _stopFilter)
            {
                ref var rbComponent = ref _rbPool.GetRef(entity);
                rbComponent.LinearVelocity = Vector2.zero;
            }
        }

        private void SetLinearVelToBody(int entity)
        {
            
            var moveDirectionComponent = _moveDirectionPool.Get(entity);
            var speedComponent = _speedPool.Get(entity);
            ref var rbComponent = ref _rbPool.GetRef(entity);
            rbComponent.LinearVelocity =
                new Vector2(moveDirectionComponent.value.x, moveDirectionComponent.value.z).normalized *
                speedComponent.Value;
        }

        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            _rbPool = world.GetPool<Box2DRigidbodyComponent>();
            _movingPool = world.GetPool<MovingComponent>();
            _moveDirectionPool = world.GetPool<MoveDirectionComponent>();
            _speedPool = world.GetPool<AverageSpeedComponent>();
            _cantMovePool = world.GetPool<CantMoveComponent>();

            _moveFilter = world
                .Filter<MoveDirectionComponent>()
                .Inc<AverageSpeedComponent>()
                .Inc<UnitComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .End();

            _stopFilter = world
                .Filter<UnitComponent>()
                .Exc<MoveDirectionComponent>()
                .Exc<TargetPositionComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .End();

        }
    }
}