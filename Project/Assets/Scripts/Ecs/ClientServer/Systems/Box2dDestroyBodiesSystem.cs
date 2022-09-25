using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class Box2DDestroyBodiesSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filter;
        private EcsPool<Box2DBodyComponent> _poolBodies;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _poolBodies = _world.GetPool<Box2DBodyComponent>();
            
            //_filter = _world.FilterBase().Inc<DeletedEntityComponent>().Inc<Box2DBodyComponent>().End();
            _filter = _world.FilterMarkedDeleted().Inc<Box2DBodyComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                Box2DApiSafe.DestroyBody(_poolBodies.Get(entity).BodyReference);
                _poolBodies.Del(entity);
            }
        }
    }
}