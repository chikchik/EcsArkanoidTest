using System;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Random = System.Random;

namespace Game.Ecs.ClientServer.Systems
{
    public class GameSpawnSystem : IEcsInitSystem, IEcsRunSystem
    {
        private enum BrickSpawnFigure
        {
            Full,
            Chessboard
        }

        private readonly GameEntityFactory _gameEntityFactory;
        private readonly Vector3 _platformSpawnPosition = new Vector3(0f, 0.5f, -8f);
        private readonly Vector3 _ballSpawnPosition = new Vector3(0f, 0f, 0f);
        private readonly Vector2Int _gridSize = new Vector2Int(10, 6);
        private readonly Vector3 _gridTopLeftCorner = new Vector3(-9f, 0.5f, 9.5f);

        private EcsWorld _world;
        private EcsFilter _filterNewReadyPlayers;
        private EcsFilter _filterGamePlayers;
        private EcsFilter _filterBricks;
        private EcsFilter _filterBalls;
        private EcsFilter _filterAutoCleanable;
        private EcsFilter _filterGamePlaying;
        private EcsFilter _filterGameWaitingStart;
        private EcsFilter _filterGamePlayingStart;
        private EcsFilter _filterGameOverStart;

        private readonly Random _random = new Random(System.Environment.TickCount);

        public GameSpawnSystem(GameEntityFactory gameEntityFactory)
        {
            _gameEntityFactory = gameEntityFactory;
        }

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();

            _filterNewReadyPlayers = _world.Filter<PlayerComponent>()
                .Inc<NewPlayerComponent>()
                .Inc<NicknameComponent>().End();

            _filterGamePlayers = _world.Filter<PlayerComponent>().Inc<NicknameComponent>().End();
            _filterBricks = _world.Filter<BrickComponent>().End();
            _filterBalls = _world.Filter<BallComponent>().End();
            _filterAutoCleanable = _world.Filter<AutoCleanableComponent>().End();
            _filterGamePlaying = _world.Filter<GamePlayingComponent>().End();
            _filterGameWaitingStart = _world.Filter<GameWaitingComponent>().IncAdded<GameWaitingComponent>().End();
            _filterGamePlayingStart = _world.Filter<GamePlayingComponent>().IncAdded<GamePlayingComponent>().End();
            _filterGameOverStart = _world.Filter<GameOverComponent>().IncAdded<GameOverComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var playerEntity in _filterNewReadyPlayers)
            {
                int unitEntity = _gameEntityFactory.CreatePlatform(_world, playerEntity);
                unitEntity.EntityGetRef<PositionComponent>(_world).Value = _platformSpawnPosition;

                playerEntity.EntityDel<NewPlayerComponent>(_world);

                foreach (int _ in _filterGamePlaying)
                {
                    SetEnablePlayerMove(playerEntity, true);
                }
            }

            foreach (int _ in _filterGameWaitingStart)
            {
                ClearBoard();
            }

            foreach (int _ in _filterGamePlayingStart)
            {
                var array = Enum.GetValues(typeof(BrickSpawnFigure));
                RespawnBricks((BrickSpawnFigure)array.GetValue(_random.Next(array.Length)));
                RespawnBall();
                SetEnablePlayersMove(true);
            }

            foreach (int _ in _filterGameOverStart)
            {
                SetEnablePlayersMove(false);
            }
        }

        private void ClearBoard()
        {
            foreach (int entity in _filterAutoCleanable)
            {
                _world.MarkEntityAsDeleted(entity);
            }
        }

        private void ClearBricks()
        {
            foreach (int entity in _filterBricks)
            {
                _world.MarkEntityAsDeleted(entity);
            }
        }

        private void ClearBalls()
        {
            foreach (int entity in _filterBalls)
            {
                _world.MarkEntityAsDeleted(entity);
            }
        }

        private void SetEnablePlayersMove(bool enableMove)
        {
            foreach (int entity in _filterGamePlayers)
            {
                SetEnablePlayerMove(entity, enableMove);
            }
        }

        private void SetEnablePlayerMove(int entity, bool enableMove)
        {
            if (enableMove)
            {
                if (entity.EntityHas<CantMoveComponent>(_world))
                {
                    entity.EntityDel<CantMoveComponent>(_world);
                }
            }
            else
            {
                entity.EntityGetOrCreateRef<CantMoveComponent>(_world);
            }
        }

        private void RespawnBricks(BrickSpawnFigure figure)
        {
            ClearBricks();

            if (figure == BrickSpawnFigure.Full)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    for (int y = 0; y < _gridSize.y; y++)
                    {
                        _gameEntityFactory.CreateBrick(_world, GridCellPosition(x, y), 1f);
                    }
                }
            }
            else if (figure == BrickSpawnFigure.Chessboard)
            {
                for (int x = 0; x < _gridSize.x; x++)
                {
                    for (int y = 0; y < _gridSize.y; y++)
                    {
                        if (x % 2 == y % 2)
                        {
                            _gameEntityFactory.CreateBrick(_world, GridCellPosition(x, y), 1f);
                        }
                    }
                }
            }
        }

        private void RespawnBall()
        {
            ClearBalls();

            var dir = new Vector2(_random.Range(-1, 1), _random.Range(-1, 1)).normalized;
            _gameEntityFactory.CreateBall(_world, _ballSpawnPosition, dir * 200f);
        }

        private Vector3 GridCellPosition(int x, int y) => new Vector3(
            _gridTopLeftCorner.x + _gameEntityFactory.BrickSize.x * x,
            _gridTopLeftCorner.y,
            _gridTopLeftCorner.z - _gameEntityFactory.BrickSize.y * y);
    }
}