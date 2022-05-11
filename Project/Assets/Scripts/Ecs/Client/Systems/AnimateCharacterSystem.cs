using System;
using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    [Serializable]
    public struct AnimationStateComponent
    {
        public string id;
    } 
    
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

                
                
                //animator.SetBool("idle", Mathf.Approximately(moveDirectionComponent.value.magnitude, 0));
                
                animator.SetFloat("Blend", moveDirectionComponent.value.magnitude);
                animator.SetFloat("speed", speedComponent.speed / 2f);
            }

            filter = world.Filter<AnimationStateComponent>().IncChanges<AnimationStateComponent>().End();

            foreach (var entity in filter)
            {
                var state = entity.EntityGet<AnimationStateComponent>(world).id;
                var animatorComponent = poolAnimator.Get(entity);
                animatorComponent.animator.SetTrigger(state);
            }
        }
    }
}