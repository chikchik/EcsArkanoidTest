using System;
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
        private EcsFilter _filterReadyPlayers;
        private EcsFilter _filterBricks;
        private EcsFilter _filterBalls;
        private EcsFilter _filterGameState;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            var gameEntity = _world.NewEntity();
            gameEntity.EntityAdd<GameStateComponent>(_world);
            gameEntity.EntityAdd<GameTimeComponent>(_world);

            _filterReadyPlayers = _world.Filter<PlayerComponent>().Inc<NicknameComponent>().End();
            _filterBricks = _world.Filter<BrickComponent>().End();
            _filterBalls = _world.Filter<BallComponent>().End();
            _filterGameState = _world.Filter<GameStateComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterGameState)
            {
                var currentGameState = entity.EntityGet<GameStateComponent>(_world).Value;
                switch (currentGameState)
                {
                    case GameState.None:
                        if (_filterReadyPlayers.GetEntitiesCount() > 0)
                        {
                            ChangeGameState(entity, GameState.Init);
                        }
                        break;

                    case GameState.Init:
                        if (_world.GetTime() - entity.EntityGet<GameTimeComponent>(_world).StartTime >= 1f)
                        {
                            ChangeGameState(entity, GameState.Play);
                        }
                        break;

                    case GameState.Play:
                        if (_filterReadyPlayers.GetEntitiesCount() == 0)
                        {
                            ChangeGameState(entity, GameState.None);
                        }
                        else if (_filterBricks.GetEntitiesCount() == 0 ||
                                 _filterBalls.GetEntitiesCount() == 0)
                        {
                            ChangeGameState(entity, GameState.End);
                        }
                        break;

                    case GameState.End:
                        if (_world.GetTime() - entity.EntityGet<GameTimeComponent>(_world).StartTime >= 3f)
                        {
                            ChangeGameState(entity, GameState.None);
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ChangeGameState(int entity, GameState newGameState)
        {
            entity.EntityGetRef<GameStateComponent>(_world).Value = newGameState;
            entity.EntityGetRef<GameTimeComponent>(_world).StartTime = _world.GetTime();
        }
    }
}