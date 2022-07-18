using Fabros.Ecs.Utils;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;

namespace Fabros.EcsModules.Mech.Client.Systems
{
    public class MechAnimationSystem: IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<MechComponent>().Inc<MechMovingComponent>().End();
            foreach (var entity in filter)
            {
                var animator = entity.EntityGet<MechAnimatorComponent>(world).Animator;
                //animator.Play("");
            }
        }
    }
}