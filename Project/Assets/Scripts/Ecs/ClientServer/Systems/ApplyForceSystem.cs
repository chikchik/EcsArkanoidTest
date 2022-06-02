using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyForceSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            
            var world = systems.GetWorld();
            var worldReference = world.GetUnique<Box2DWorldComponent>().WorldReference;

            var poolApplyForce = world.GetPool<ApplyForceComponent>();
            
            var filter = world.Filter<ApplyForceComponent>().End();
            foreach (var entity in filter)
            {
                var time = poolApplyForce.Get(entity).Time;
                if (time > world.GetTime())
                    continue;
                
                var pos = entity.EntityGet<PositionComponent>(world).value;
                var dir = poolApplyForce.Get(entity).Direction;// entity.EntityGet<LookDirectionComponent>(world).value;
                
                Box2DApi.RaycastOutputReturnType ret = new Box2DApi.RaycastOutputReturnType();
                if (Box2DApi.RayCast(worldReference, pos.ToVector2XZ(),
                        dir.ToVector2XZ(), ref ret, 1))
                {
                    //Debug.Log("raycast ok");
                    var contactPos = ret.Point;
                    var body = ret.Body;
                    Box2DApi.ApplyForce(body, dir.ToVector2XZ() * 100, contactPos);
                    if (world.GetDebugName() == "copyServerWorld")
                    {
                        int w = 0;
                    }
                    Debug.Log($"Apply Force {world.GetDebugName()} - tick={world.GetUnique<TickComponent>().Value}  at {contactPos} from {pos}");
                }
                
                poolApplyForce.Del(entity);
            }
        }
    }
}