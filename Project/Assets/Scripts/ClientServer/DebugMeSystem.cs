using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.ClientServer.Utils;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    /*
     * отладочная система для поиска расинхрона, добавляется в список систем автоматически
     */
    public class DebugMeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private bool pre;
        private EcsWorld world;
        private EcsFilter filter;
        
        public DebugMeSystem(bool pre)
        {
            this.pre = pre;
        }
        
        public void Run(EcsSystems systems)
        {
            //if logging disabled skip
            if (world.GetSyncLogger() == null)
                return;
            
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

        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            filter = world.Filter<DebugMeComponent>().End();
        }
    }
}