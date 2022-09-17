using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;

namespace Game.Ecs.Client.Systems
{
    public class DeadWorldClearSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _deadWorld;
        private int[] entities = null;
        
        public void Init(EcsSystems systems)
        {
            _deadWorld = systems.GetWorld(EcsWorlds.Dead);
        }
        
        public void Run(EcsSystems systems)
        {
            int count = _deadWorld.GetAllEntities(ref entities);
            for (int i = 0; i < count; ++i)
            {
                _deadWorld.DelEntity(entities[i]);
            }
        }
    }
}