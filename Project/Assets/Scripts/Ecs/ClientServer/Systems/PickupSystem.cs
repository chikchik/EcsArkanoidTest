using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class PickupSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsWorld _eventWorld;

        private EcsPool<PickupableComponent> _poolPickupable;
        private EcsPool<PickuperComponent> _poolPickuper;
        private EcsPool<PickupedComponent> _poolPickuped;
        private EcsPool<Box2DBeginContactComponent> _poolContacts;
        private EcsFilter _filterContacts;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);

            _poolPickupable = _world.GetPool<PickupableComponent>();
            _poolPickuper = _world.GetPool<PickuperComponent>();
            _poolPickuped = _world.GetPool<PickupedComponent>();
            _poolContacts = _eventWorld.GetPool<Box2DBeginContactComponent>();
            _filterContacts = _eventWorld.Filter<Box2DBeginContactComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterContacts)
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
            if (_poolPickupable.Has(entityA) &&
                !_poolPickuped.Has(entityA) &&
                _poolPickuper.Has(entityB))
            {
                _poolPickuper.Del(entityA);
                _poolPickuped.Add(entityA).PickuperEntity = _world.PackEntity(entityB);
                entityA.EntityAdd<DestroyAtTimeComponent>(_world).TimeValue = _world.GetTime() + 0.01f;
            }
        }
    }
}