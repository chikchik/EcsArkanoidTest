using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class AIPlayerSystem : IEcsInitSystem, IEcsRunSystem
    {
        private Random _random;
        
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsPool<TargetPositionComponent> _poolTargetPosition;

        public AIPlayerSystem()
        {
            _random = new Random(2);
        }
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            
            _filter = _world
                .Filter<AIPlayerComponent>()
                .Exc<TargetPositionComponent>()
                .End();
            _poolTargetPosition = _world.GetPool<TargetPositionComponent>();
        }

        public void Run(EcsSystems systems)
        {
            _random = new Random(_world.GetTick());
            
            foreach (var entity in _filter)
            {
                var position = new Vector3
                {
                    x = GetRandom(-10, 4.5f),
                    y = 0,
                    z = GetRandom(5, 22f)
                };

                ref var targetPositionComponent = ref _poolTargetPosition.Add(entity);
                targetPositionComponent.Value = position;
            }
        }

        private float GetRandom(float min, float max)
        {
            var range = max - min;
            var randomValue = _random.NextDouble();
            var rangedValue = randomValue * range + min;

            return (float) rangedValue;
        }
    }
}