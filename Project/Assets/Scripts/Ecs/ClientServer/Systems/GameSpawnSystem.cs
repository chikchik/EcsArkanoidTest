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
        private readonly Vector3 platformSpawnPosition = new Vector3(0f, 0.5f, -8f);
        private readonly Vector3 ballSpawnPosition = new Vector3(0f, 0f, 0f);
        private readonly Vector2Int gridSize = new Vector2Int(10, 6);
        private readonly Vector3 gridTopLeftCorner = new Vector3(-9f, 0.5f, 9.5f);

        private EcsWorld _world;
        private EcsFilter _filterNewReadyPlayers;
        private EcsFilter _filterGamePlayers;
        private EcsFilter _filterBricks;
        private EcsFilter _filterBalls;
        private EcsFilter _filterAutoCleanable;
        private EcsFilter _filterGameState;
        private EcsFilter _filterGameStateChanges;

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
            _filterGameState = _world.Filter<GameStateComponent>().End();
            _filterGameStateChanges = _world.Filter<GameStateComponent>().IncChanges<GameStateComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var playerEntity in _filterNewReadyPlayers)
            {
                int unitEntity = _gameEntityFactory.CreatePlatform(_world, playerEntity);
                unitEntity.EntityGetRef<PositionComponent>(_world).Value = platformSpawnPosition;

                playerEntity.EntityDel<NewPlayerComponent>(_world);

                foreach (int gameEntity in _filterGameState)
                {
                    var gameState = gameEntity.EntityGet<GameStateComponent>(_world).Value;
                    SetEnablePlayerMove(playerEntity, gameState == GameState.Play);
                }
            }

            foreach (int entity in _filterGameStateChanges)
            {
                var gameState = entity.EntityGet<GameStateComponent>(_world).Value;
                if (gameState == GameState.None)
                {
                    ClearBoard();
                }
                else if (gameState == GameState.Init)
                {
                    var array = Enum.GetValues(typeof(BrickSpawnFigure));
                    RespawnBricks((BrickSpawnFigure)array.GetValue(_random.Next(array.Length)));
                }
                else if (gameState == GameState.Play)
                {
                    RespawnBall();
                }
                else if (gameState == GameState.End)
                {
                    ClearBoard();
                }

                SetEnablePlayersMove(gameState == GameState.Play);
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
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        _gameEntityFactory.CreateBrick(_world, GridCellPosition(x, y), 1f);
                    }
                }
            }
            else if (figure == BrickSpawnFigure.Chessboard)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
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
            _gameEntityFactory.CreateBall(_world, ballSpawnPosition, dir * 200f);
        }

        private Vector3 GridCellPosition(int x, int y) => new Vector3(
            gridTopLeftCorner.x + _gameEntityFactory.BrickSize.x * x,
            gridTopLeftCorner.y,
            gridTopLeftCorner.z - _gameEntityFactory.BrickSize.y * y);
    }
}