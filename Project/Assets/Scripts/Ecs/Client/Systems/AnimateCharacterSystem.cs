using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class AnimateCharacterSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .Inc<MoveDirectionComponent>()
                .Inc<AnimatorComponent>()
                .End();
            var poolAnimator = world.GetPool<AnimatorComponent>();
            var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolSpeed = world.GetPool<SpeedComponent>();
            //var context = world.GetUnique<LeoSharedComponent>().Context;

            foreach (var entity in filter)
            {
                var animatorComponent = poolAnimator.Get(entity);
                var speedComponent = poolSpeed.Get(entity);
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                var animator = animatorComponent.animator;


                animator.SetBool("idle", Mathf.Approximately(moveDirectionComponent.value.magnitude, 0));
                animator.SetFloat("speed", speedComponent.speed / 2f);

                /*
             var playerID = entity.EntityGetComponent<LeoPlayerComponent>(world).id;
            if (Services.HasInteractionForPlayerId(world, playerID))
            {
                animator.CrossFade("Pick Up", 0.05f);
            }*/
            }
        }
    }
}