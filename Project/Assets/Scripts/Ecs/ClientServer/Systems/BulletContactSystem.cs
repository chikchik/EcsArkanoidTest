﻿using Game.Ecs.ClientServer.Components;

using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Utils;

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
            foreach (var entity in filter)
            {
                var contact = poolContacts.Get(entity);
                OnBulletContact(contact.Data.EntityA);
                OnBulletContact(contact.Data.EntityB);
            }
        }

        private void OnBulletContact(EcsPackedEntity packedEntity)
        {
            int entity;
            if (!packedEntity.Unpack(world, out entity))
                return;
            
            if (!world.EntityHas<BulletComponent>(entity))
                return;
            world.DelEntity(entity);
        }
    }
}