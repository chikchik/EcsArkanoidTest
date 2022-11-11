using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Components.Other;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class BallContactSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorld _world;
        private EcsWorld _eventWorld;

        private EcsPool<HpComponent> _poolHp;
        private EcsPool<BallComponent> _poolBall;
        private EcsPool<DestroyComponent> _poolDestroy;
        private EcsPool<Box2DBeginContactComponent> _poolContacts;
        private EcsFilter _filterContacts;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _eventWorld = systems.GetWorld(EcsWorlds.Event);

            _poolHp = _world.GetPool<HpComponent>();
            _poolBall = _world.GetPool<BallComponent>();
            _poolDestroy = _world.GetPool<DestroyComponent>();
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
            if (_poolBall.Has(entityA) && _poolHp.Has(entityB) && !_poolDestroy.Has(entityB))
            {
                ref var hpComponent = ref _poolHp.GetRef(entityB);
                hpComponent.Value -= 1f;

                if (hpComponent.Value <= 0f)
                {
                    _poolDestroy.Add(entityB);
                    //_world.MarkEntityAsDeleted(entityB);
                    entityB.EntityAdd<DestroyAtTimeComponent>(_world).TimeValue = _world.GetTime() + 0.01f;
                }
            }
        }
    }
}