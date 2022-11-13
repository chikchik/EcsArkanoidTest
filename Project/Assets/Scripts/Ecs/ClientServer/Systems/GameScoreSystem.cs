using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class GameScoreSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filterDestroyedBricks;
        private EcsFilter _filterScore;
        private EcsFilter _filterGamePlayingStart;

        private const int DESTROY_BRICK_SCORE = 10;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _filterDestroyedBricks = _world.Filter<BrickComponent>().IncAdded<DestroyComponent>().End();
            _filterScore = _world.Filter<ScoreComponent>().End();
            _filterGamePlayingStart = _world.Filter<GamePlayingComponent>().IncAdded<GamePlayingComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (int gameEntity in _filterGamePlayingStart)
            {
                gameEntity.EntityGetRef<ScoreComponent>(_world).Value = 0;
            }

            foreach (int _ in _filterDestroyedBricks)
            {
                foreach (int gameEntity in _filterScore)
                {
                    gameEntity.EntityGetRef<ScoreComponent>(_world).Value += DESTROY_BRICK_SCORE;
                }
            }
        }
    }
}