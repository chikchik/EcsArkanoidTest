using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;
using Object = System.Object;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class PhysicsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _ecsWorld;
        private Box2DPhysics.CallbackDelegate _cbkBeginContactDelegate;
        private Box2DPhysics.CallbackDelegate _cbkEndContactDelegate;
        private Box2DPhysics.CallbackDelegate _cbkPostSolveDelegate;
        private Box2DPhysics.CallbackDelegate _cbkPreSolveDelegate;
        
        public void Init(EcsSystems systems)
        {
            _ecsWorld = systems.GetWorld();
            if (!_ecsWorld.HasUnique<PhysicsWorldComponent>())
                _ecsWorld.AddUnique<PhysicsWorldComponent>().WorldReference = Box2DPhysics.CreateWorld(Config.GRAVITY);

            var b2world = _ecsWorld.GetUnique<PhysicsWorldComponent>().WorldReference;

            _cbkBeginContactDelegate = SetBeginContactCallback;
            _cbkEndContactDelegate = SetEndContactCallback;
            _cbkPostSolveDelegate = SetPreSolveCallback;
            _cbkPreSolveDelegate = SetPostSolveCallback;
            
            Box2DPhysics.SetBeginContactCallback(b2world, _cbkBeginContactDelegate);
            Box2DPhysics.SetEndContactCallback(b2world, _cbkEndContactDelegate);
            Box2DPhysics.SetPostSolveCallback(b2world, _cbkPostSolveDelegate);
            Box2DPhysics.SetPreSolveCallback(b2world, _cbkPreSolveDelegate);
            
        }
        
        public void SetBeginContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<BeginContactComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetEndContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<EndContactComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPreSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PreSolveComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPostSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PostSolveComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            if(!world.HasUnique<PhysicsWorldComponent>()) 
                return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var filter = world
                .Filter<RigidbodyDefinitionComponent>()
                .Inc<RotationComponent>()
                .Exc<BodyCreatedComponent>()
                .End();

            var poolRigidbodyDefinition = world.GetPool<RigidbodyDefinitionComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<RotationComponent>();
            
            var poolBoxCollider = world.GetPool<BoxColliderComponent>();
            var poolCircleCollider = world.GetPool<CircleColliderComponent>();
            var poolPolygonCollider = world.GetPool<PolygonColliderComponent>();
            var poolChainCollider = world.GetPool<ChainColliderComponent>();
            
            var poolBodyCreated = world.GetPool<BodyCreatedComponent>();

            foreach (var entity in filter)
            {
                poolBodyCreated.Add(entity);
                
                var rigidbodyDefinitionComponent = poolRigidbodyDefinition.Get(entity);
                var positionComponent = poolPositionComponent.Get(entity);
                var bodyAngle = poolRotationComponent.Get(entity).value;
                
                var bodyReference = Box2DPhysics.CreateBody(
                    physicsWorld.WorldReference,
                    (int) rigidbodyDefinitionComponent.BodyType,
                    new Vector2(positionComponent.value.x, positionComponent.value.z),
                    bodyAngle,
                    entity);
                
                //var bodyReference =  (physicsWorld, rigidbodyDefinitionComponent,
                //    positionComponent, bodyAngle, entity);
                
                IntPtr shape = IntPtr.Zero;
                if (poolBoxCollider.Has(entity))
                {
                    shape = Box2DPhysics.CreateBoxShape(poolBoxCollider.Get(entity).Size / 2f);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                    Box2DPhysics.SetAngularDamping(bodyReference, rigidbodyDefinitionComponent.AngularDamping);
                }
                else if (poolCircleCollider.Has(entity))
                {
                    shape = Box2DPhysics.CreateCircleShape(poolCircleCollider.Get(entity).Radius);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                }
                else if (poolPolygonCollider.Has(entity))
                {
                    var vertices = poolPolygonCollider.Get(entity).Vertices;
                    var anchors = poolPolygonCollider.Get(entity).Anchors;

                    int index = 0;
                    List<Vector2> shapeVertices = new List<Vector2>();
                    foreach (var anchor in anchors)
                    {
                        shapeVertices.Clear();
                        for (int offset = index; index <= anchor + offset; index++)
                        {
                            shapeVertices.Add(vertices[index]);
                        }
                        shape = Box2DPhysics.CreatePolygonShape(shapeVertices.ToArray(), shapeVertices.Count);
                        AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                    }
                }
                else if (poolChainCollider.Has(entity))
                {
                    var vertices = poolChainCollider.Get(entity).Points;
                    
                    shape = Box2DPhysics.CreateChainShape(vertices, vertices.Length);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                }
                else
                {
                    throw new Exception("missing collider");
                }
                
                Box2DPhysics.SetLinearDamping(bodyReference, rigidbodyDefinitionComponent.LinearDamping);
                ref var bodyReferenceComponent = ref entity.EntityAddComponent<BodyReferenceComponent>(world);
                bodyReferenceComponent.BodyReference = bodyReference;

                ref var rigidbodyComponent = ref entity.EntityReplace<RigidbodyComponent>(world);
                rigidbodyComponent.BodyType = rigidbodyDefinitionComponent.BodyType;
            }
        }

        private void AddFixtureToBody(IntPtr bodyReference, IntPtr shape,
            RigidbodyDefinitionComponent rigidbodyDefinitionComponent)
        {
            if (shape == IntPtr.Zero) return;

            B2Filter filter;
            filter.CategoryBits = rigidbodyDefinitionComponent.CategoryBits;
            filter.MaskBits = rigidbodyDefinitionComponent.MaskBits;
            filter.GroupIndex = rigidbodyDefinitionComponent.GroupIndex;
            
            Box2DPhysics.AddFixtureToBody(bodyReference, shape,
                rigidbodyDefinitionComponent.Density,
                rigidbodyDefinitionComponent.Friction,
                rigidbodyDefinitionComponent.Restitution,
                rigidbodyDefinitionComponent.RestitutionThreshold,
                rigidbodyDefinitionComponent.IsTrigger,
                filter);
        }
    }
}
