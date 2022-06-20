using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Systems;
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
            //var poolSpeed = world.GetPool<SpeedComponent>();
            //var context = world.GetUnique<LeoSharedComponent>().Context;

            foreach (var entity in filter)
            {
                var animatorComponent = poolAnimator.Get(entity);
                //var speedComponent = poolSpeed.Get(entity);
                var moveDirectionComponent = poolMoveDirection.Get(entity);
                var animator = animatorComponent.animator;
                

                //animator.SetBool("idle", Mathf.Approximately(moveDirectionComponent.value.magnitude, 0));
                
                //animator.SetFloat("Blend", moveDirectionComponent.value.magnitude);
                //animator.SetFloat("speed", speedComponent.speed / 2f);
            }

        
            filter = world.Filter<FoodCollectedComponent>().IncChanges<FoodCollectedComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).animator;
                animator.CrossFadeInFixedTime("gather", 0.1f);
            }
            
            
            filter = world.Filter<UnitComponent>().IncAdded<MovingComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).animator;
                var current = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                if (current != "walking")
                    animator.CrossFadeInFixedTime("walking", 0.05f);
            }
            
            filter = world.Filter<UnitComponent>().Inc<MovingComponent>().IncRemoved<CantMoveComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).animator;
                var current = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                if (current != "walking")
                    animator.CrossFadeInFixedTime("walking", 0.05f);
            }
            
            filter = world.Filter<UnitComponent>().IncRemoved<MovingComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).animator.CrossFadeInFixedTime("idle", 0.1f);
            }

            filter = world.Filter<UnitComponent>().IncAdded<PushingComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).animator.CrossFadeInFixedTime("kicking", 0.05f);
            }
            
            filter = world.Filter<UnitComponent>().Inc<ShootStartedComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).animator.CrossFadeInFixedTime("Shooting", 0.05f);
            }
        }
    }
}