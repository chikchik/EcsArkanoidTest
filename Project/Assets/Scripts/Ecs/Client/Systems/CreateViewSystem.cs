using Game.Ecs.ClientServer.Components;
using Game.View;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;
using Zenject;

namespace Game.Ecs.Client.Systems
{
    public class CreateViewSystem : IEcsInitSystem, IEcsRunSystem
    {
        [Inject] private PlatformView _platformPrefab;
        [Inject] private BrickView _brickPrefab;
        [Inject] private BallView _ballPrefab;
        [Inject] private BonusView _bonusView;

        private EcsWorld _world;
        private EcsFilter _filterGameObjectName;
        private EcsFilter _filterUnits;
        private EcsFilter _filterBricks;
        private EcsFilter _filterBalls;
        private EcsFilter _filterBonuses;

        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _filterGameObjectName = _world.Filter<GameObjectNameComponent>()
                .Exc<TransformComponent>().End();

            _filterUnits = _world.Filter<UnitComponent>()
                .Exc<TransformComponent>().End();

            _filterBricks = _world.Filter<BrickComponent>()
                .Exc<TransformComponent>().End();

            _filterBalls = _world.Filter<BallComponent>()
                .Exc<TransformComponent>().End();

            _filterBonuses = _world.Filter<BonusComponent>()
                .Inc<PickupableComponent>()
                .Exc<TransformComponent>().End();
        }

        public void Run(EcsSystems systems)
        {
            foreach (var entity in _filterGameObjectName)
            {
                var name = entity.EntityGet<GameObjectNameComponent>(_world).Name.ToString();
                var go = GameObject.Find(name);
                if (go == null)
                {
                    Debug.LogError($"not found gameobject {name} for entity {entity.e2name(_world)}");
                    continue;
                }
                go.SetActive(true);

                if (entity.EntityTryGet(_world, out PositionComponent pos))
                {
                    entity.EntityAdd<TransformComponent>(_world).Transform = go.transform;
                    go.transform.position = pos.Value;
                }

                if (entity.EntityTryGet(_world, out Rotation2DComponent rot))
                {
                    var angle = rot.AngleRadians * -Mathf.Rad2Deg;
                    go.transform.eulerAngles = go.transform.eulerAngles.WithY(angle);
                }
            }

            foreach (var entity in _filterUnits)
            {
                var view = Object.Instantiate(_platformPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(_world).Value;

                entity.EntityAdd<TransformComponent>(_world).Transform = view.transform;
                entity.EntityGetOrCreateRef<LerpComponent>(_world).Value = 0.5f;

                if (entity.EntityGet<UnitComponent>(_world).PlayerEntity.Unpack(_world, out int playerEntity) &&
                    playerEntity.EntityTryGet(_world, out NicknameComponent nicknameComponent))
                {
                    view.Nickname.gameObject.SetActive(true);
                    view.Nickname.text = nicknameComponent.Value;
                }
            }

            foreach (var entity in _filterBricks)
            {
                var view = Object.Instantiate(_brickPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(_world).Value;

                entity.EntityAdd<TransformComponent>(_world).Transform = view.transform;
            }


            foreach (var entity in _filterBalls)
            {
                var view = Object.Instantiate(_ballPrefab);
                view.transform.position = entity.EntityGet<PositionComponent>(_world).Value;

                entity.EntityAdd<TransformComponent>(_world).Transform = view.transform;
            }

            foreach (var entity in _filterBonuses)
            {
                var view = Object.Instantiate(_bonusView);
                view.transform.position = entity.EntityGet<PositionComponent>(_world).Value;

                entity.EntityAdd<TransformComponent>(_world).Transform = view.transform;
            }
        }
    }
}