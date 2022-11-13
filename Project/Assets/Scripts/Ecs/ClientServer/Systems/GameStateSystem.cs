using Game.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class GameStateSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _filterReadyPlayer;
        private EcsFilter _filterBrick;
        private EcsFilter _filterBall;
        private EcsFilter _filterGameWaiting;
        private EcsFilter _filterGamePlaying;
        private EcsFilter _filterGameOver;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            var gameEntity = _world.NewEntity();
            gameEntity.EntityAdd<GameComponent>(_world);
            gameEntity.EntityAdd<ScoreComponent>(_world);
            gameEntity.EntityAdd<GameWaitingComponent>(_world);

            _filterReadyPlayer = _world.Filter<PlayerComponent>().Inc<NicknameComponent>().End();
            _filterBrick = _world.Filter<BrickComponent>().End();
            _filterBall = _world.Filter<BallComponent>().End();
            _filterGameWaiting = _world.Filter<GameWaitingComponent>().End();
            _filterGamePlaying = _world.Filter<GamePlayingComponent>().End();
            _filterGameOver = _world.Filter<GameOverComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterGameOver)
            {
                if (_world.GetTime() - entity.EntityGet<GameTimeComponent>(_world).StartTime >= 2f)
                {
                    entity.EntityDel<GameTimeComponent>(_world);
                    entity.EntityDel<GameOverComponent>(_world);
                    entity.EntityAdd<GameWaitingComponent>(_world);
                }
            }

            foreach (var entity in _filterGamePlaying)
            {
                if (_filterReadyPlayer.GetEntitiesCount() == 0)
                {
                    entity.EntityDel<GamePlayingComponent>(_world);
                    entity.EntityAdd<GameWaitingComponent>(_world);
                }
                else if (_filterBrick.GetEntitiesCount() == 0 ||
                         _filterBall.GetEntitiesCount() == 0)
                {
                    entity.EntityDel<GamePlayingComponent>(_world);
                    entity.EntityAdd<GameTimeComponent>(_world).StartTime = _world.GetTime();
                    entity.EntityAdd<GameOverComponent>(_world);
                }
            }

            foreach (var entity in _filterGameWaiting)
            {
                if (_filterReadyPlayer.GetEntitiesCount() > 0)
                {
                    entity.EntityDel<GameWaitingComponent>(_world);
                    entity.EntityAdd<GamePlayingComponent>(_world);
                }
            }
        }
    }
}