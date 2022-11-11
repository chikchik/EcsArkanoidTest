using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class BoardBoundTrackingSystem : IEcsInitSystem, IEcsRunSystem
    {
        private readonly Vector2Int gridSize = new Vector2Int(10, 6);
        private Vector3 min = new Vector3(-10f, -3f, -10f);
        private Vector3 max = new Vector3(10f, 3f, 10f);

        private EcsWorld _world;
        private EcsFilter _filterTrackable;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _filterTrackable = _world.Filter<BoardBoundsTrackableComponent>()
                .Inc<PositionComponent>()
                .End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterTrackable)
            {
                var position = entity.EntityGet<PositionComponent>(_world).Value;
                if (position.x < min.x || position.x > max.x ||
                    position.y < min.y || position.y > max.y ||
                    position.z < min.z || position.z > max.z)
                {
                    _world.MarkEntityAsDeleted(entity);
                }
            }
        }
    }
}