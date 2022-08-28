using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Net.ClientServer;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld world;
        private EcsWorld eventWorld;
        
        private EcsPool<DestructibleHealthComponent> poolDestructibleHealth;
        private EcsPool<BulletComponent> poolBullet;
        private EcsPool<BulletHitComponent> poolBulletHits;
        private EcsPool<Box2DBeginContactComponent> poolContacts;
        private EcsFilter filter;
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            eventWorld = systems.GetWorld(EcsWorlds.Event);
            
            poolBulletHits = eventWorld.GetPool<BulletHitComponent>();
            
            poolDestructibleHealth = world.GetPool<DestructibleHealthComponent>();
            poolBullet = world.GetPool<BulletComponent>();
            poolContacts = eventWorld.GetPool<Box2DBeginContactComponent>();
            filter = eventWorld.Filter<Box2DBeginContactComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);

                if (!contact.Data.EntityA.Unpack(world, out var entityA))
                {
                    Debug.Log($"contact {entity} entityA dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (!contact.Data.EntityB.Unpack(world, out var entityB))
                {
                    Debug.Log($"contact {entity} entityB dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (entityA == entityB)
                {
                    //контакт сам с собой???
                    Debug.LogError($"self contact wtf {entityA}");
                    continue;
                }

                
                Check(entityA, entityB);
                Check(entityB, entityA);
            }
        }

        private void Check(int entityA, int entityB)
        {
            BulletComponent bulletComponent;
            if (poolBullet.TryGet(entityA, out bulletComponent))
            {
                if (poolDestructibleHealth.Has(entityB))
                {
                    var hitEntity = eventWorld.NewEntity();
                    
                    ref var bulletHit = ref poolBulletHits.Add(hitEntity);
                    bulletHit.Bullet = bulletComponent;
                    bulletHit.EntityHit = world.PackEntity(entityB);
                }
                world.DelEntity(entityA);
            }
        }
    }
}