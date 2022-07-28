using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.EcsLite;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class AIPlayerSystem : IEcsRunSystem
    {
        private readonly Random random;

        public AIPlayerSystem()
        {
            random = new Random(2);
        }

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<AIPlayerComponent>()
                .Exc<TargetPositionComponent>()
                .End();
            var poolTargetPosition = world.GetPool<TargetPositionComponent>();

            foreach (var entity in filter)
            {
                var position = new Vector3
                {
                    x = GetRandom(-4.25f, 6.5f),
                    y = 0,
                    z = GetRandom(-2f, 18f)
                };

                ref var targetPositionComponent = ref poolTargetPosition.Add(entity);
                targetPositionComponent.Value = position;
            }
        }

        private float GetRandom(float min, float max)
        {
            var range = max - min;
            var randomValue = random.NextDouble();
            var rangedValue = randomValue * range + min;

            return (float) rangedValue;
        }
    }
}