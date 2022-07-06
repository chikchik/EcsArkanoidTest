using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        EcsWorld world;
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
        }
        
        public void Run(EcsSystems systems)
        {
            
            var filter = world.Filter<Box2DBeginContactComponent>().End();
            var poolContacts = world.GetPool<Box2DBeginContactComponent>();
            var poolBullets = world.GetPool<BulletComponent>();
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);
                var entityA = contact.CollisionCallbackData.EntityA;
                var entityB = contact.CollisionCallbackData.EntityB;
                if (poolBullets.Has(entityA))
                    OnBulletContact(entityA);
                if (poolBullets.Has(entityB))
                    OnBulletContact(entityB);
            }
        }

        private void OnBulletContact(int entity)
        {
            world.DelEntity(entity);
        }
    }
}