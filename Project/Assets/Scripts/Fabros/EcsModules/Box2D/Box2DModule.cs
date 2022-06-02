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
            pool.AddComponent<Box2DRigidbodyDefinitionComponent>();
            pool.AddComponent<Box2DRigidbodyComponent>();
            pool.AddComponent<Box2DBoxColliderComponent>();
            pool.AddComponent<Box2DCircleColliderComponent>();
            pool.AddComponent<Box2DPolygonColliderComponent>();
            pool.AddComponent<Box2DChainColliderComponent>();
            pool.AddComponent<Rotation2DComponent>();
            pool.AddComponent<JointTestComponent>();
        }

        /*
        public static IEcsSystem[] GetSystems()
        {
            //not implemented        
        }*/
    }
}