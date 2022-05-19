using System;
using System.Collections.Generic;
using System.Linq;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Physics;
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

                var position = entity.EntityGet<PositionComponent>(world).value;
                entity.EntityAdd<RootMotionComponent>(world).Position = position;

                transformComponent.transform.position = position;

                entity.EntityReplaceComponent<LerpComponent>(world).value = 0.5f;
            }
        }

        private static void forEachObject<T>(Action<T> fn) where T : Component//it is UNITY Mono Component
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

            forEachObject<BoxView>(view =>
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

            forEachObject<ButtonView>(view =>
            {
                var buttonEntity = GetOrCreateGameEntity(view.gameObject);
                ref var buttonComponent = ref buttonEntity.EntityAdd<ButtonComponent>(world);
                buttonComponent.isActivated = false;

                ref var positionComponent = ref buttonEntity.EntityAdd<PositionComponent>(world);
                positionComponent.value = view.transform.position;

                ref var radiusComponent = ref buttonEntity.EntityAdd<RadiusComponent>(world);
                radiusComponent.radius = view.transform.lossyScale.x / 2f;

                buttonEntity.EntityAdd<InteractableComponent>(world);
                

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

                
                //characterEntity.EntityAddComponent<AverageSpeedComponent>(world).value = clip.averageSpeed;

                ref var radiusComponent = ref characterEntity.EntityAddComponent<RadiusComponent>(world);
                radiusComponent.radius = 0.4f;

                ref var speedComponent = ref characterEntity.EntityAddComponent<SpeedComponent>(world);
                speedComponent.speed = 2f;
            });
            
            forEachObject<Collider2D>(collider =>
            {
                var go = collider.gameObject;
                var rigidBody = go.GetComponent<Rigidbody2D>();
                if (!rigidBody) 
                    return;
                
                var entity = GetOrCreateGameEntity(collider.gameObject);

                entity.EntityReplace<PositionComponent>(world).value = go.transform.position;
                entity.EntityReplace<RotationComponent>(world).value = - go.transform.eulerAngles.y * Mathf.Deg2Rad;

                ref var rigidBodyDefinitionComponent =
                    ref entity.EntityAddComponent<RigidbodyDefinitionComponent>(world);
                
                if (go.TryGetComponent(out Box2dPhysicsExtend rigidbodyExtend))
                {
                    rigidBodyDefinitionComponent.Density = rigidbodyExtend.Density;
                    rigidBodyDefinitionComponent.RestitutionThreshold = rigidbodyExtend.RestitutionThreshold;
                    rigidBodyDefinitionComponent.LinearDamping = rigidbodyExtend.LinearDamping;
                    rigidBodyDefinitionComponent.AngularDamping = rigidbodyExtend.AngularDamping;
                }

                var type = rigidBody.bodyType;
                var b2Type = BodyType.Static;
                if (type == RigidbodyType2D.Dynamic)
                    b2Type = BodyType.Dynamic;
                else if (type == RigidbodyType2D.Kinematic)
                    b2Type = BodyType.Kinematic;

                rigidBodyDefinitionComponent.BodyType = b2Type;
                var material = rigidBody.sharedMaterial;
                if (material)
                {
                    rigidBodyDefinitionComponent.Friction = material.friction;
                    var sharedMaterialBounciness = rigidBodyDefinitionComponent.Restitution = material.bounciness;
                }

                rigidBodyDefinitionComponent.IsTrigger = collider.isTrigger;
                
                
                if (collider is BoxCollider2D)
                {
                    ref var boxColliderComponent = ref entity.EntityAddComponent<BoxColliderComponent>(world);
                    boxColliderComponent.Size = go.transform.lossyScale;
                }

                if (collider is CircleCollider2D)
                {
                    ref var circleColliderComponent = ref entity.EntityAddComponent<CircleColliderComponent>(world);
                    circleColliderComponent.Radius = collider.transform.lossyScale.x / 2;
                }

                if (collider is PolygonCollider2D)
                {
                    ref var polygonColliderComponent = ref entity.EntityAddComponent<PolygonColliderComponent>(world);
                    PhysicsShapeGroup2D physicsShapeGroup2D = new PhysicsShapeGroup2D();
                    var shapes = collider.GetShapes(physicsShapeGroup2D);

                    polygonColliderComponent.Anchors = new int[shapes];
                    polygonColliderComponent.Vertices = new List<Vector2>();
                    var vertices = new List<Vector2>();
                    for (int i = 0; i < shapes; i++)
                    {
                        physicsShapeGroup2D.GetShapeVertices(i, vertices);
                        polygonColliderComponent.Anchors[i] = vertices.Count - 1;
                        foreach (var vector2 in vertices)
                        {
                            polygonColliderComponent.Vertices.Add(vector2);
                        }
                    }
                }
                
                

                DeleteFromViewIfPlaying(go.gameObject, collider, rigidBody);
            });
            
            /*
            forEachObject<CubeView>(cubeView =>
            {
                var rigidBodyEntity = GetOrCreateGameEntity(cubeView.gameObject);

                AddTransformComponentsToEntityFromView(world, rigidBodyEntity, cubeView.transform);
                
                var rigidBody = cubeView.GetComponent<Rigidbody2D>();

                cubeView.TryGetComponent(out Box2dPhysicsExtend rigidbodyExtend);
                
                var boxCollider = cubeView.GetComponent<BoxCollider2D>();
                
                AddRbDefinitionComponentFromUnity(world, rigidBodyEntity, rigidBody, boxCollider, rigidbodyExtend);
                if (boxCollider)
                {
                    ref var boxColliderComponent = ref rigidBodyEntity.EntityAddComponent<BoxColliderComponent>(world);
                    boxColliderComponent.Size = cubeView.transform.lossyScale;
                }

                DeleteFromViewIfPlaying(cubeView.gameObject, boxCollider, rigidBody);
            });
            
            
            
            /*
            forEachObject<SphereView>(sphereView =>
            {
                var rigidBodyEntity = GetOrCreateGameEntity(sphereView.gameObject);

                AddTransformComponentsToEntityFromView(world, rigidBodyEntity, sphereView.transform);
                
                var rigidBody = sphereView.GetComponent<Rigidbody2D>();
                
                
                var circleCollider = sphereView.GetComponent<CircleCollider2D>();
                AddRbDefinitionComponentFromUnity(world, rigidBodyEntity, rigidBody, circleCollider);
                if (circleCollider)
                {
                    ref var circleColliderComponent = ref rigidBodyEntity.EntityAddComponent<CircleColliderComponent>(world);
                    circleColliderComponent.Radius = sphereView.transform.lossyScale.x / 2;
                }
                
                DeleteFromViewIfPlaying(sphereView.gameObject, circleCollider, rigidBody);
            });
            
            forEachObject<PolygonView>(polygonView =>
            {
                var rigidBodyEntity = GetOrCreateGameEntity(polygonView.gameObject);

                AddTransformComponentsToEntityFromView(world, rigidBodyEntity, polygonView.transform);

                var rigidBody = polygonView.GetComponent<Rigidbody2D>();
                
                
                var polygonCollider2D = polygonView.GetComponent<PolygonCollider2D>();
                AddRbDefinitionComponentFromUnity(world, rigidBodyEntity, rigidBody, polygonCollider2D);
                if (polygonCollider2D)
                {
                    ref var polygonColliderComponent = ref rigidBodyEntity.EntityAddComponent<PolygonColliderComponent>(world);
                    PhysicsShapeGroup2D physicsShapeGroup2D = new PhysicsShapeGroup2D();
                    var shapes = polygonCollider2D.GetShapes(physicsShapeGroup2D);

                    polygonColliderComponent.Anchors = new int[shapes];
                    polygonColliderComponent.Vertices = new List<Vector2>();
                    var vertices = new List<Vector2>();
                    for (int i = 0; i < shapes; i++)
                    {
                        physicsShapeGroup2D.GetShapeVertices(i, vertices);
                        polygonColliderComponent.Anchors[i] = vertices.Count - 1;
                        foreach (var vector2 in vertices)
                        {
                            polygonColliderComponent.Vertices.Add(vector2);
                        }
                    }
                }
                
                DeleteFromViewIfPlaying(polygonView.gameObject, polygonCollider2D, rigidBody);
            });
            
            forEachObject<ChainView>(staticWallView =>
            {
                var rigidBodyEntity = GetOrCreateGameEntity(staticWallView.gameObject);

                AddTransformComponentsToEntityFromView(world, rigidBodyEntity, staticWallView.transform);

                var rigidBody = staticWallView.GetComponent<Rigidbody2D>();
                var polygonCollider2D = staticWallView.GetComponent<PolygonCollider2D>();
                
                AddRbDefinitionComponentFromUnity(world, rigidBodyEntity, rigidBody, polygonCollider2D);
                
                if (polygonCollider2D)
                {
                    ref var chainColliderComponent =
                        ref rigidBodyEntity.EntityAddComponent<ChainColliderComponent>(world);
                    chainColliderComponent.Points = polygonCollider2D.points;
                }
                
                DeleteFromViewIfPlaying(staticWallView.gameObject, polygonCollider2D, rigidBody);
            });*/

            void DeleteFromViewIfPlaying(GameObject view, Collider2D collider2D,
                Rigidbody2D rigidBody)
            {
                if (Application.IsPlaying(view))
                {
                    Object.DestroyImmediate(collider2D);
                    Object.DestroyImmediate(rigidBody);
                }
            }
            
            var unit = Object.FindObjectOfType<Global>().characterPrefab;
            
            var clips = unit.Animator.runtimeAnimatorController.animationClips;
            var clip = clips.First(clip => clip.name == "Walking");
            //todo calculate exact speed

            world.AddUnique<AverageSpeedComponent>().Value = 1.77f; //clip.averageSpeed;
        }
    }
}