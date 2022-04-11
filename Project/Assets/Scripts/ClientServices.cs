using System;
using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Game.Utils;
using Game.View;
using Leopotam.EcsLite;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game.Client
{
    public class ClientServices
    {
        public static void ConfigureCollectableUnit(EcsWorld world, GameObject gameObject, int entity)
        {
            var bushView = gameObject.GetComponent<BushView>();
            ref var collectableTargetComponent = ref entity.EntityAddComponent<CollectableTargetComponent>(world);

            collectableTargetComponent.targetObject = bushView.Berries.gameObject;
        }

        public static void LinkUnits(EcsWorld world)
        {
            var viewComponent = world.GetUnique<ClientViewComponent>();

            //клиентский код, он привязывает GameObjects к entities  из мира
            //вызывается часто и должно быть быстрым, корректно обрабатывать появление новых entities
            //и пропадание каких-то старых
            //потому тут на всякий случай передается прошлый уже привязанный мир к game objects 
            //что может облегчить задачу

            var filter = world.Filter<GameObjectNameComponent>().End();

            foreach (var entity in filter)
            {
                if (entity.EntityHasComponent<GameObjectComponent>(world))
                    continue;

                if (entity.EntityHasComponent<GameObjectNameComponent>(world))
                {
                    var name = entity.EntityGetComponent<GameObjectNameComponent>(world).Name;
                    var go = GameObject.Find(name).gameObject;
                    go.gameObject.SetActive(true);

                    entity.EntityAddComponent<GameObjectComponent>(world).GameObject = go;
                    entity.EntityAddComponent<TransformComponent>(world).transform = go.transform;

                    if (entity.EntityHasComponent<CollectableComponent>(world))
                        ConfigureCollectableUnit(world, go, entity);
                }
            }

            var filterUnits = world.Filter<UnitComponent>().End();

            foreach (var entity in filterUnits)
            {
                if (entity.EntityHasComponent<GameObjectComponent>(world))
                    continue;

                Debug.Log($"link {entity}");

                var characterView = Object.Instantiate(viewComponent.Global.characterPrefab);

                ref var component = ref entity.EntityAddComponent<GameObjectComponent>(world);
                component.GameObject = characterView.gameObject;

                ref var transformComponent = ref entity.EntityAddComponent<TransformComponent>(world);
                transformComponent.transform = characterView.transform;

                ref var animatorComponent = ref entity.EntityAddComponent<AnimatorComponent>(world);
                animatorComponent.animator = characterView.Animator;

                ref var headComponent = ref entity.EntityAddComponent<HeadComponent>(world);
                headComponent.head = characterView.Head;

                entity.EntityReplaceComponent<LerpComponent>(world).value = 0.2f;
            }
        }

        private static void forEachObject<T>(Action<T> fn) where T : MonoBehaviour
        {
            var items = Object.FindObjectsOfType<T>();
            items.ForEach(fn);
        }

        public static void InitializeNewWorldFromScene(EcsWorld world)
        {
            var entityCollectionSystem = world.NewEntity();
            ref var entityCollectionComponent = ref entityCollectionSystem.EntityAdd<EntityCollectionComponent>(world);
            var entities = entityCollectionComponent.entities = new Dictionary<GameObject, int>();

            int GetOrCreateGameEntity(GameObject go)
            {
                if (entities.TryGetValue(go, out var entity)) return entity;

                entity = world.NewEntity();

                ref var component = ref entity.EntityAdd<GameObjectComponent>(world);
                component.GameObject = go;

                ref var gameObjectNameComponent = ref entity.EntityAdd<GameObjectNameComponent>(world);
                gameObjectNameComponent.Name = go.name;

                ref var transformComponent = ref entity.EntityAdd<TransformComponent>(world);
                transformComponent.transform = go.transform;

                entities.Add(go, entity);

                return entity;
            }


            forEachObject<BushView>(view =>
            {
                var bushEntity = GetOrCreateGameEntity(view.gameObject);
                ref var bushComponent = ref bushEntity.EntityAdd<BushComponent>(world);
                bushComponent.name = "Berries";

                ref var positionComponent = ref bushEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var interactableComponent = ref bushEntity.EntityAdd<InteractableComponent>(world);
                interactableComponent.isInteractable = true;
                interactableComponent.canInteract = false;

                ref var radiusComponent = ref bushEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;

                ref var resourceComponent = ref bushEntity.EntityAdd<ResourceComponent>(world);
                resourceComponent.count = 1;

                ref var collectableComponent = ref bushEntity.EntityAdd<CollectableComponent>(world);
                collectableComponent.isCollected = false;

                ref var collectableTargetComponent = ref bushEntity.EntityAdd<CollectableTargetComponent>(world);
                collectableTargetComponent.targetObject = view.Berries.gameObject;

                bushEntity.EntityAdd<FlammableComponent>(world).Power = 3;
            });

            forEachObject<BoxView>(view =>
            {
                var boxEntity = GetOrCreateGameEntity(view.gameObject);
                boxEntity.EntityAdd<BoxComponent>(world);

                var forward = view.transform.forward;
                var position = view.transform.position + forward / 2;

                ref var positionComponent = ref boxEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = position;

                ref var interactableComponent = ref boxEntity.EntityAdd<InteractableComponent>(world);
                interactableComponent.isInteractable = true;
                interactableComponent.canInteract = false;

                ref var radiusComponent = ref boxEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;
            });

            forEachObject<ButtonView>(view =>
            {
                var buttonEntity = GetOrCreateGameEntity(view.gameObject);
                ref var buttonComponent = ref buttonEntity.EntityAdd<ButtonComponent>(world);
                buttonComponent.isActivated = false;

                ref var positionComponent = ref buttonEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var radiusComponent = ref buttonEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;

                ref var interactableComponent = ref buttonEntity.EntityAdd<InteractableComponent>(world);
                interactableComponent.isInteractable = true;
                interactableComponent.canInteract = false;

                ref var speedComponent = ref buttonEntity.EntityAddComponent<SpeedComponent>(world);
                speedComponent.speed = view.speed;

                ref var progressComponent = ref buttonEntity.EntityAddComponent<ProgressComponent>(world);
                progressComponent.progress = 0;

                ref var moveInfoComponent = ref buttonEntity.EntityAddComponent<MoveInfoComponent>(world);
                moveInfoComponent.startPoint = view.StartPosition;
                moveInfoComponent.endPoint = view.EndPosition;
            });

            forEachObject<GateView>(view =>
            {
                var gateEntity = GetOrCreateGameEntity(view.gameObject);
                ref var gateComponent = ref gateEntity.EntityAdd<GateComponent>(world);

                ref var buttonLinkComponent = ref gateEntity.EntityAdd<ButtonLinkComponent>(world);
                buttonLinkComponent.buttonIds = new int[view.OpenedBy.Count];

                for (var i = 0; i < view.OpenedBy.Count; i++)
                    if (entities.TryGetValue(view.OpenedBy[i], out var buttonEntity))
                        buttonLinkComponent.buttonIds[i] = buttonEntity;

                ref var positionComponent = ref gateEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var radiusComponent = ref gateEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = 1f;

                ref var speedComponent = ref gateEntity.EntityAdd<SpeedComponent>(world);
                speedComponent.speed = view.OpenSpeed;

                ref var progressComponent = ref gateEntity.EntityAddComponent<ProgressComponent>(world);
                progressComponent.progress = 0;

                ref var moveInfoComponent = ref gateEntity.EntityAddComponent<MoveInfoComponent>(world);
                moveInfoComponent.startPoint = view.StartPosition;
                moveInfoComponent.endPoint = view.EndPosition;
            });

            forEachObject<CharacterView>(view =>
            {
                var characterEntity = GetOrCreateGameEntity(view.gameObject);

                ref var playerComponent = ref characterEntity.EntityAddComponent<PlayerComponent>(world);
                playerComponent.id = Random.Range(-9999, -1111);

                //if (view.IsAICharacter) characterEntity.EntityAddComponent<AIPlayerComponent>(world);

                characterEntity.EntityAddComponent<UnitComponent>(world);

                characterEntity.EntityAddComponent<MoveDirectionComponent>(world);
                characterEntity.EntityAddComponent<PositionComponent>(world);

                ref var radiusComponent = ref characterEntity.EntityAddComponent<RadiusComponent>(world);
                radiusComponent.radius = 0.4f;

                ref var speedComponent = ref characterEntity.EntityAddComponent<SpeedComponent>(world);
                speedComponent.speed = 2f;
            });
        }
    }
}