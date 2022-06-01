using Fabros.Ecs.ClientServer.Serializer;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Grid.Systems;
using Leopotam.EcsLite;

namespace Fabros.EcsModules.Box2D
{
    public static class Box2DModule
    {
        public static void AddSerializableComponents(ComponentsPool pool)
        {
            pool.AddComponent<RigidbodyDefinitionComponent>();
            pool.AddComponent<RigidbodyComponent>();
            pool.AddComponent<BoxColliderComponent>();
            pool.AddComponent<CircleColliderComponent>();
            pool.AddComponent<PolygonColliderComponent>();
            pool.AddComponent<ChainColliderComponent>();
            pool.AddComponent<RotationComponent>();
            pool.AddComponent<JointTestComponent>();
        }

        /*
        public static IEcsSystem[] GetSystems()
        {
            //not implemented        
        }*/
    }
}