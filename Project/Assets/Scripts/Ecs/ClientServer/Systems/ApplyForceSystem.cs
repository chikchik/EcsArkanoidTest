using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyForceSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsPool<ApplyForceComponent> _poolApplyForce;
        private EcsFilter _filter;
        private EcsWorld _world;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _poolApplyForce = _world.GetPool<ApplyForceComponent>();
            _filter = _world.Filter<ApplyForceComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var worldReference = _world.GetUnique<Box2DWorldComponent>().WorldReference;


            foreach (var entity in _filter)
            {
                var time = _poolApplyForce.Get(entity).Time;
                if (time > _world.GetTime())
                    continue;
                
                var pos = entity.EntityGet<PositionComponent>(_world).Value;
                var dir = _poolApplyForce.Get(entity).Direction;
                
                Box2DApiTypes.RaycastOutputReturnType ret = new Box2DApiTypes.RaycastOutputReturnType();
                if (Box2DApiInternal.RayCast(worldReference, pos.ToVector2XZ(),
                        dir.ToVector2XZ(), ref ret, 1))
                {
                    var contactPos = ret.Point;
                    var body = ret.Body;
                    Box2DApiSafe.ApplyForce(body, dir.ToVector2XZ() * 100, contactPos);
                }
                
                _poolApplyForce.Del(entity);
            }
        }
    }
}