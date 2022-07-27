using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class MechAdapterSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            //данная система превращает ключевые игровые компоненты во внутрениие важные компоненты мех модуля
            //
            var world = systems.GetWorld();
            var filter = world.Filter<MechComponent>().End();
            foreach (var entity in filter)
            {
                if (entity.EntityHas<MovingComponent>(world))
                    entity.EntityGetOrCreateRef<MechMovingComponent>(world);
                else
                    entity.EntityDel<MechMovingComponent>(world);
                
                //if (entity.EntityHas<LookDirectionComponent>(world))
                //  entity.EntityGetOrCreateRef<LookDirectionComponent>(world) = entity.E;
                
            }
        }
    }
}