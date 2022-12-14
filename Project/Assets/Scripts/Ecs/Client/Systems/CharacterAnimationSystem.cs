using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Systems;

using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.EcsLite;

namespace Game.Ecs.Client.Systems
{
    public class CharacterAnimationSystem : IEcsRunSystem
    {
        private void ContinueAnimation(Animator animator, string anim)
        {
            var currentAnimatorClipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (currentAnimatorClipInfo.Length != 0)
            {
                var current = currentAnimatorClipInfo[0].clip.name;
                if (current != anim)
                    animator.CrossFadeInFixedTime(anim, 0.05f);
                return;
            }
            
            animator.CrossFadeInFixedTime(anim, 0.05f);
        }
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var poolAnimator = world.GetPool<AnimatorComponent>();
        
            var filter = world.Filter<FoodCollectedComponent>().IncChanges<FoodCollectedComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).Animator;
                animator.CrossFadeInFixedTime("gather", 0.1f);
            }
            
            
            filter = world.Filter<UnitComponent>().IncAdded<MovingComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).Animator;
                ContinueAnimation(animator, "walking");
            }
            
            filter = world.Filter<UnitComponent>().Inc<MovingComponent>().IncRemoved<CantMoveComponent>().End();

            foreach (var entity in filter)
            {
                var animator = poolAnimator.Get(entity).Animator;
                ContinueAnimation(animator, "walking");
            }
            
            filter = world.Filter<UnitComponent>().IncRemoved<MovingComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).Animator.CrossFadeInFixedTime("idle", 0.1f);
            }

            filter = world.Filter<UnitComponent>().IncAdded<PushingComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).Animator.CrossFadeInFixedTime("kicking", 0.05f);
            }
            
            filter = world.Filter<UnitComponent>().Inc<ShootStartedComponent>().End();
            foreach (var entity in filter)
            {
                poolAnimator.Get(entity).Animator.CrossFadeInFixedTime("Shooting", 0.1f);
            }
        }
    }
}