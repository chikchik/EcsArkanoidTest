using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Net.ClientServer;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class BulletContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsWorld _eventWorld;
        
        private EcsPool<HpComponent> _poolDestructibleHealth;
        private EcsPool<BulletComponent> _poolBullet;
        private EcsPool<BulletHitComponent> _poolBulletHits;
        private EcsPool<Box2DBeginContactComponent> _poolContacts;
        private EcsFilter _filter;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);
            
            _poolBulletHits = _eventWorld.GetPool<BulletHitComponent>();
            
            _poolDestructibleHealth = _world.GetPool<HpComponent>();
            _poolBullet = _world.GetPool<BulletComponent>();
            _poolContacts = _eventWorld.GetPool<Box2DBeginContactComponent>();
            _filter = _eventWorld.Filter<Box2DBeginContactComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filter)
            {
                var contact = _poolContacts.Get(entity);

                if (!contact.Data.EntityA.Unpack(_world, out var entityA))
                {
                    Debug.Log($"contact {entity} entityA dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (!contact.Data.EntityB.Unpack(_world, out var entityB))
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
            if (_poolBullet.TryGet(entityA, out bulletComponent))
            {
                if (_poolDestructibleHealth.Has(entityB))
                {
                    var hitEntity = _eventWorld.NewEntity();
                    
                    ref var bulletHit = ref _poolBulletHits.Add(hitEntity);
                    bulletHit.Bullet = bulletComponent;
                    bulletHit.EntityHit = _world.PackEntity(entityB);
                }
                _world.DelEntityByComponent(entityA);
                //_world.DelEntity(entityA);
            }
        }
    }
}