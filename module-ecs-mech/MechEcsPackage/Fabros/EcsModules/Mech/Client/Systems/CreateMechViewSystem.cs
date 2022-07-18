using Fabros.Ecs.Client.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;

namespace Fabros.EcsModules.Mech.Client.Systems
{
    public class CreateMechViewSystem: IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        private EcsFilter filter;
        private Animator Prefab;
        
        public void Init(EcsSystems systems)
        {
            Prefab = Resources.Load<Animator>("Mech");
            world = systems.GetWorld();
            filter = world.Filter<MechComponent>()
                .Exc<TransformComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                var animator = GameObject.Instantiate(Prefab);
                entity.EntityAdd<TransformComponent>(world).Transform = animator.transform;
                entity.EntityAdd<MechAnimatorComponent>(world).Animator = animator;
            }
        }
    }
}