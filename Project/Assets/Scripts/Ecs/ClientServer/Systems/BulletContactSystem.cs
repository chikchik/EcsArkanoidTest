using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
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
        private EcsPool<BulletDamageComponent> _poolBullet;
        private EcsPool<BulletHitComponent> _poolBulletHits;
        private EcsPool<Box2DBeginContactComponent> _poolContacts;
        private EcsFilter _filter;
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);
            
            _poolBulletHits = _eventWorld.GetPool<BulletHitComponent>();
            
            _poolDestructibleHealth = _world.GetPool<HpComponent>();
            _poolBullet = _world.GetPool<BulletDamageComponent>();
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
                    _world.Log($"contact {entity} entityA dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (!contact.Data.EntityB.Unpack(_world, out var entityB))
                {
                    _world.Log($"contact {entity} entityB dead {contact.Data.EntityA.ToString()}");
                    continue;
                }

                if (entityA == entityB)
                {
                    //контакт сам с собой???
                    _world.LogError($"self contact wtf {entityA}");
                    continue;
                }

                
                Check(entityA, entityB);
                Check(entityB, entityA);
            }
        }

        private void Check(int entityA, int entityB)
        {
            BulletDamageComponent bulletComponent;
            if (_poolBullet.TryGet(entityA, out bulletComponent))
            {
                if (_poolDestructibleHealth.Has(entityB))
                {
                    var hitEntity = _eventWorld.NewEntity();
                    
                    ref var bulletHit = ref _poolBulletHits.Add(hitEntity);
                    bulletHit.Bullet = bulletComponent;
                    bulletHit.EntityHit = _world.PackEntity(entityB);
                }
                _world.MarkEntityAsDeleted(entityA);
                //_world.DelEntity(entityA);
            }
        }
    }
}