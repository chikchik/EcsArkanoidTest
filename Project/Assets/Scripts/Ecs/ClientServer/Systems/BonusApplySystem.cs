using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Utils;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class BonusApplySystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly GameEntityFactory _gameEntityFactory;

        private EcsWorld _world;
        private EcsFilter _filterPickupedBonus;

        private readonly Random _random = new Random(System.Environment.TickCount);

        public BonusApplySystem(GameEntityFactory gameEntityFactory)
        {
            _gameEntityFactory = gameEntityFactory;
        }

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _filterPickupedBonus = _world.Filter<BonusComponent>()
                .IncAdded<PickupedComponent>()
                .End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var bonusEntity in _filterPickupedBonus)
            {
                var bonusType = bonusEntity.EntityGet<BonusComponent>(_world).BonusType;
                if (bonusType == BonusType.MultiBalls)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        var dir = new Vector2(_random.Range(-1, 1), _random.Range(-1, 1)).normalized;
                        _gameEntityFactory.CreateBall(_world, Vector3.zero, dir * 200f);
                    }
                }
            }
        }
    }
}