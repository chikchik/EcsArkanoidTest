using System;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class BonusSpawnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameEntityFactory _gameEntityFactory;

        private EcsWorld _world;
        private EcsFilter _filterDestroyedBricks;
        private Random _random = new Random(Environment.TickCount);

        public BonusSpawnSystem(GameEntityFactory gameEntityFactory)
        {
            _gameEntityFactory = gameEntityFactory;
        }

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _filterDestroyedBricks = _world.Filter<BrickComponent>()
                .IncAdded<DestroyComponent>()
                .End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var brickEntity in _filterDestroyedBricks)
            {
                if (_random.Next(10) % 5 == 0)
                {
                    var position = brickEntity.EntityGet<PositionComponent>(_world).Value;
                    _gameEntityFactory.CreateDropBonus(_world, position, BonusType.MultiBalls);
                }
            }
        }
    }
}