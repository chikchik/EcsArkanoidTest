using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class DebugMeSystem : IEcsRunSystem
    {
        private bool pre;
        public DebugMeSystem(bool pre)
        {
            this.pre = pre;
        }
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world.Filter<DebugMeComponent>().End();
            foreach (var entity in filter)
            {
                var str = "";
                if (pre)
                    str += "beg-";
                else
                    str += "end-";
                
                if (entity.EntityHas<PositionComponent>(world))
                    str += entity.EntityGet<PositionComponent>(world).value;
                if (entity.EntityHas<Box2DRigidbodyComponent>(world))
                    str += entity.EntityGet<Box2DRigidbodyComponent>(world).LinearVelocity;
                world.Log(str);
            }
        }
    }
}