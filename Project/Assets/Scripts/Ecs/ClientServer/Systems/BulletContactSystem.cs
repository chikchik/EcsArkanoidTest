using Fabros.Ecs.Utils;
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
                
                OnBulletContact(entityA);
                OnBulletContact(entityB);
            }
        }

        private void OnBulletContact(int entity)
        {
            //todo remove: world.IsEntityAliveInternal usage
            if (!world.IsEntityAliveInternal(entity))
                return;
            if (!world.EntityHas<BulletComponent>(entity))
                return;
            world.DelEntity(entity);
        }
    }
}