using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Client
{
    public class DebugDraw
    {
        public static void Draw(EcsWorld world)
        {
            var filter = world.Filter<PositionComponent>().End();

            var poolPos = world.GetPool<PositionComponent>();

            foreach (var entity in filter)
            {
                var pos = poolPos.Get(entity).value;

                Gizmos.color = Color.red;

                if (!entity.EntityHas<FootprintComponent>(world))
                {
                    pos = pos.WithAddedToY(1.0f);
                    Gizmos.color = Color.white;
                }
                
                if (entity.EntityHas<UnitComponent>(world))
                {
                    Gizmos.color = Color.green;
//                    Debug.Log($"unit {entity} {pos}");
                }


                Gizmos.DrawWireSphere(pos, 0.2f);

                entity.EntityWith<MoveDirectionComponent>(world, data =>
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(pos, data.value);
                });

                entity.EntityWith<ApplyForceComponent>(world, data =>
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(pos, data.Direction);
                });
            }
        }
    }
}