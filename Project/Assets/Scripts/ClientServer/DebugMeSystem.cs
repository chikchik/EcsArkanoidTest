using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Net.ClientServer;
using XFlow.Utils;

namespace Game.ClientServer
{
    /*
     * отладочная система для поиска расинхрона, добавляется в список систем автоматически
     */
    public class DebugMeSystem : IEcsInitSystem, IEcsRunSystem
    {
        private bool _pre;
        private EcsWorld _world;
        private EcsFilter _filter;
        
        public DebugMeSystem(bool pre)
        {
            this._pre = pre;
        }
        
        public void Run(EcsSystems systems)
        {
            //if logging disabled skip
            if (_world.GetSyncLogger() == null)
                return;
            
            foreach (var entity in _filter)
            {
                var str = "";
                if (_pre)
                    str += "beg-";
                else
                    str += "end-";

                str += $"{entity.e2name(_world, false, false)}-";

                if (entity.EntityHas<PositionComponent>(_world))
                    str += entity.EntityGet<PositionComponent>(_world).value;
                if (entity.EntityHas<Box2DLinearVelocityComponent>(_world))
                    str += entity.EntityGet<Box2DLinearVelocityComponent>(_world).Value;

                _world.LogVerbose(str);
            }
        }

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filter = _world.Filter<DebugMeComponent>().End();
        }
    }
}