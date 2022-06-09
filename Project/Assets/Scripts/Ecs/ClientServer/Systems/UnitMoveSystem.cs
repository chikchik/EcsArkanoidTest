using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class UnitMoveSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            return;
            var world = systems.GetWorld();
            var filter = world
                .Filter<MoveDirectionComponent>()
                .Inc<SpeedComponent>()
                .Inc<UnitComponent>()
                .Inc<Box2DBodyComponent>()
                .End();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            var poolBodyReference = world.GetPool<Box2DBodyComponent>();

            foreach (var entity in filter)
            {
                SetLinearVelToBody(entity, poolBodyReference, poolMoveDirection, poolSpeed);
            }
        }

        private void SetLinearVelToBody(int entity, EcsPool<Box2DBodyComponent> poolBodyReference,
            EcsPool<MoveDirectionComponent> poolMoveDirection, EcsPool<SpeedComponent> poolSpeed)
        {
            var bodyReference = poolBodyReference.Get(entity).BodyReference;
            var moveDirectionComponent = poolMoveDirection.Get(entity);
            var speedComponent = poolSpeed.Get(entity);

            Box2DApi.SetLinearVelocity(bodyReference,
                new Vector2(moveDirectionComponent.value.x, moveDirectionComponent.value.z) *
                speedComponent.speed);
        }
    }
}