
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Utils
{
    public class EcsWorldDebugDraw
    {
        public static void Draw(EcsWorld world)
        {
            var filter = world.Filter<PositionComponent>().End();

            var poolPos = world.GetPool<PositionComponent>();

            foreach (var entity in filter)
            {
                var pos = poolPos.Get(entity).value;

                Gizmos.color = Color.red;

                var rad = 0.1f;

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

                if (entity.EntityHas<BulletComponent>(world))
                    rad = 0.02f;


                Gizmos.DrawWireSphere(pos, rad);

                entity.EntityWith<MoveDirectionComponent>(world, data =>
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(pos, data.value);
                });
                
                entity.EntityWith<LookDirectionComponent>(world, data =>
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(pos.WithAddedToY(-0.02f), data.value);
                });

                entity.EntityWith<ApplyForceComponent>(world, data =>
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(pos, data.Direction);
                });

                /*
                entity.EntityWith<Rotation2DComponent>(world, data =>
                {
                    var dir = Quaternion.Euler(0,  data.Angle * Mathf.Rad2Deg, 0) * new Vector3(0, 0, 1f);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(pos, dir);
                });
                */
            }
        }
    }
}