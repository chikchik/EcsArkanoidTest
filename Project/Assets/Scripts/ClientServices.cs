using System;
using System.Collections.Generic;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.View;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.Client;
using XFlow.Modules.Box2D.ClientServer.Components.Joints;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game
{
    public class ClientServices
    {
        private static void ForEachObject<T>(Action<T> fn) where T : Component//it is UNITY Mono Component
        {
            var items = Object.FindObjectsOfType<T>();
            foreach (var item in items)
            {
                fn(item);
            }
        }

        public static void InitializeNewWorldFromScene(EcsWorld world)
        {
            var entities = new Dictionary<GameObject, int>();

            //int NameId = 0;
            int GetOrCreateGameEntity(GameObject go)
            {
                if (entities.TryGetValue(go, out var entity)) return entity;

                entity = world.NewEntity();
                //go.name = $"{go.name}[{entity}]";
                
                
                entity.EntityAdd<DebugNameComponent>(world).Name = go.name;
                
                
                ref var gameObjectNameComponent = ref entity.EntityAdd<GameObjectNameComponent>(world);
                gameObjectNameComponent.Name = go.name;
                //NameId++;

                ref var transformComponent = ref entity.EntityAdd<TransformComponent>(world);
                transformComponent.Transform = go.transform;

                entities.Add(go, entity);

                return entity;
            }


            ForEachObject<BushView>(view =>
            {
                var bushEntity = GetOrCreateGameEntity(view.gameObject);
                bushEntity.EntityAdd<BushComponent>(world);

                ref var positionComponent = ref bushEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                bushEntity.EntityAdd<InteractableComponent>(world);

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

            ForEachObject<BoxView>(view =>
            {
                var boxEntity = GetOrCreateGameEntity(view.gameObject);
                boxEntity.EntityAdd<BoxComponent>(world);

                var forward = view.transform.forward;
                var position = view.transform.position + forward / 2;

                ref var positionComponent = ref boxEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = position;

                boxEntity.EntityAdd<InteractableComponent>(world);

                ref var radiusComponent = ref boxEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;
            });

            ForEachObject<ButtonView>(view =>
            {
                var buttonEntity = GetOrCreateGameEntity(view.gameObject);

                ref var buttonComponent = ref buttonEntity.EntityAdd<ButtonComponent>(world);
                buttonComponent.isActivated = false;
                
                ref var positionComponent = ref buttonEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var radiusComponent = ref buttonEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;

                buttonEntity.EntityAdd<InteractableComponent>(world);
                

                ref var progressComponent = ref buttonEntity.EntityAdd<ProgressComponent>(world);
                progressComponent.progress = 0;

                ref var moveInfoComponent = ref buttonEntity.EntityAdd<MoveInfoComponent>(world);
                moveInfoComponent.startPoint = view.StartPosition;
                moveInfoComponent.endPoint = view.EndPosition;
                
                if (view.Spawner)
                    buttonEntity.EntityGetOrCreateRef<ButtonCustomComponent>(world).Spawn = true;
                if (view.Shake)
                    buttonEntity.EntityGetOrCreateRef<ButtonCustomComponent>(world).Shake = true;
            });

            ForEachObject<GateView>(view =>
            {
                var gateEntity = GetOrCreateGameEntity(view.gameObject);
                ref var gateComponent = ref gateEntity.EntityAdd<GateComponent>(world);

                ref var buttonLinkComponent = ref gateEntity.EntityAdd<ButtonLinkComponent>(world);
                buttonLinkComponent.Entities = new int[view.OpenedBy.Count];

                for (var i = 0; i < view.OpenedBy.Count; i++)
                    if (entities.TryGetValue(view.OpenedBy[i], out var buttonEntity))
                        buttonLinkComponent.Entities[i] = buttonEntity;

                ref var positionComponent = ref gateEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var radiusComponent = ref gateEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = 1f;

                ref var progressComponent = ref gateEntity.EntityAdd<ProgressComponent>(world);
                progressComponent.progress = 0;

                ref var moveInfoComponent = ref gateEntity.EntityAdd<MoveInfoComponent>(world);
                moveInfoComponent.startPoint = view.StartPosition;
                moveInfoComponent.endPoint = view.EndPosition;
            });

            ForEachObject<CharacterView>(view =>
            {
                var characterEntity = GetOrCreateGameEntity(view.gameObject);

                ref var playerComponent = ref characterEntity.EntityAdd<PlayerComponent>(world);
                playerComponent.id = Random.Range(-9999, -1111);
                characterEntity.EntityAdd<UnitComponent>(world);

                characterEntity.EntityAdd<MoveDirectionComponent>(world);
                characterEntity.EntityAdd<PositionComponent>(world);
                
                ref var radiusComponent = ref characterEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = 0.4f;
            });

            ForEachObject<DestructibleView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                entity.EntityAdd<DestructibleHealthComponent>(world).Health = 30f;
            });

            ForEachObject<Collider2D>(collider =>
            {
                if (!collider.enabled)
                    return;
                var entity = GetOrCreateGameEntity(collider.gameObject);
                ClientBox2DServices.CreateBody(world, entity, collider);
            });

            ForEachObject<JointConnectView>(joint =>
            {
                var entityA = GetOrCreateGameEntity(joint.gameObject);
                var entityB = GetOrCreateGameEntity(joint.Connect);
                entityA.EntityAdd<JointTestComponent>(world).Entity = entityB;
            });
            
            ForEachObject<SpawnGunView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                entity.EntityAdd<SpawnGunComponent>(world);
                entity.EntityAdd<InteractableComponent>(world);
                entity.EntityAdd<PositionComponent>(world).value = view.transform.position;
                
                ref var collectableComponent = ref entity.EntityAdd<CollectableComponent>(world);
                collectableComponent.isCollected = false;
                
                entity.EntityAdd<CollectableTargetComponent>(world).targetObject = view.gameObject;
            });
            
            ForEachObject<AmmoView>(view =>
            {
                var entity = GetOrCreateGameEntity(view.gameObject);
                entity.EntityAdd<AmmoComponent>(world);
                entity.EntityAdd<InteractableComponent>(world);
                entity.EntityAdd<PositionComponent>(world).value = view.transform.position;
                entity.EntityAdd<CollectableComponent>(world).isCollected = false;;
                entity.EntityAdd<CollectableTargetComponent>(world).targetObject = view.gameObject;
                entity.EntityAdd<RadiusComponent>(world).radius = view.transform.lossyScale.x / 2f;
            });
                
            var unit = Object.FindObjectOfType<Global>().CharacterPrefab;
            var clips = unit.Animator.runtimeAnimatorController.animationClips;
            //var clip = clips.First(clip => clip.name == "Walking");
            //todo calculate exact speed

            world.AddUnique<AverageSpeedComponent>().Value = 1.72f; //clip.averageSpeed;
        }
    }
}
