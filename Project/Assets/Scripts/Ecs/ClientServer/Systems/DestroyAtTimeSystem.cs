﻿using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;
#if CLIENT
using Fabros.Ecs.Client.Components;
#endif

namespace Game.Ecs.ClientServer.Systems
{
    public class DestroyAtTimeSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var tm = world.GetTime();
            
            var filter = world.Filter<DestroyAtTimeComponent>().End();

            var poolDestroyAt = world.GetPool<DestroyAtTimeComponent>();
            foreach (var entity in filter)
            {
                if (poolDestroyAt.Get(entity).Time > tm)
                    continue;
                world.Log($"time is out bullet {entity}");
                
#if CLIENT
                if (entity.EntityHasComponent<TransformComponent>(world))
                {
                    var go = entity.EntityGetComponent<TransformComponent>(world).Transform.gameObject;
                                
                    world.Log($"Delete2 Transf {go.name}");
                    Object.Destroy(go);
                }
#endif
                
                world.DelEntity(entity);
            }
        }
    }
}