using Game.ClientServer;
using Game.Ecs.View.Systems;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.Client.Systems;
using XFlow.Net.ClientServer;
using Zenject;

namespace Game.Client
{
    public class EcsViewSystemsFactory : IEcsViewSystemsFactory
    {
        private EcsSystemsContainer _container;
        
        public EcsViewSystemsFactory(DiContainer di)
        {
            _container = new EcsSystemsContainer(di);

            _container.Register<SyncTransformSystem>();
            _container.Register<RotateCharacterSystem>();
            _container.Register<RotateRigidbodySystem>();
            _container.Register<CameraFollowSystem>();
            _container.Register<Box2DDebugViewSystem>();
        }
        
        public void AddNewSystems(EcsSystems systems)
        {
            systems.Add(_container.CreateNewSystems(systems.GetWorld(), true, true));
        }
    }
}