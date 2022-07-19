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
        private GameObject Prefab;
        
        public void Init(EcsSystems systems)
        {
            Prefab = Resources.Load<GameObject>("Mech");
            world = systems.GetWorld();
            filter = world.Filter<MechComponent>()
                .Exc<TransformComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                var go = GameObject.Instantiate(Prefab);
                entity.EntityAdd<TransformComponent>(world).Transform = go.transform;

                var animator = go.GetComponentInChildren<Animator>();
                entity.EntityAdd<MechAnimatorComponent>(world).Animator = animator;
            }
        }
    }
}