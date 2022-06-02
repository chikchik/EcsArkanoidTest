using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Colliders;
using Fabros.EcsModules.Box2D.ClientServer.Components.Joints;
using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Fabros.EcsModules.Tick.Other;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.Profiling;

namespace Fabros.EcsModules.Box2D.ClientServer.Systems
{
    public class Box2DSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem, IEcsWorldChangedSystem
    {
        private Box2DApi.CollisionCallback _cbkBeginContactDelegate;
        private Box2DApi.CollisionCallback _cbkEndContactDelegate;
        private Box2DApi.CollisionCallback _cbkPostSolveDelegate;
        private Box2DApi.CollisionCallback _cbkPreSolveDelegate;
        
        private EcsWorld world;
        
        private List<Vector2> shapeVertices = new List<Vector2>();

        private bool createBodies;
        private bool updateBox2d;
        private bool updateInternalObjects;
        private bool writeToComponents;
        private bool createContacts;
        
        private int positionIterations;
        private int velocityIterations;
        private Vector2 graviti;

        public Box2DSystem(
            int positionIterations = 6,
            int velocityIterations = 2,
            Vector2 graviti = new Vector2(),
            bool createBodies = true, 
            bool updateBox2d = true, 
            bool updateInternalObjects = true,
            bool writeToComponents = true, 
            bool createContacts = true)
        {
            this.positionIterations = positionIterations;
            this.velocityIterations = velocityIterations;
            this.graviti = graviti;
            
            this.createBodies = createBodies;
            this.updateBox2d = updateBox2d;
            this.updateInternalObjects = updateInternalObjects;
            this.writeToComponents = writeToComponents;
            this.createContacts = createContacts;
        }
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            if (!world.HasUnique<Box2DWorldComponent>())
                world.AddUnique<Box2DWorldComponent>().WorldReference = Box2DApi.CreateWorld(graviti);

            var b2world = world.GetUnique<Box2DWorldComponent>().WorldReference;
            if (b2world == default)
            {
                throw new Exception("b2world is null");
            }

            if (createContacts)
            {

                _cbkBeginContactDelegate = SetBeginContactCallback;
                _cbkEndContactDelegate = SetEndContactCallback;
                _cbkPostSolveDelegate = SetPreSolveCallback;
                _cbkPreSolveDelegate = SetPostSolveCallback;

                Box2DApi.SetContactCallbacks(b2world, _cbkBeginContactDelegate, _cbkEndContactDelegate,
                    _cbkPreSolveDelegate, _cbkPostSolveDelegate);
            }
        }
        
        public void WorldChanged(EcsWorld world)
        {
            if (this.world != world)
                throw new NotImplementedException("WorldChanged not supported");
        }
        
        public void SetBeginContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<Box2DBeginContactComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetEndContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<Box2DEndContactComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPreSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<Box2DPreSolveComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPostSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<Box2DPostSolveComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }


        private void UpdateEcsComponents()
        {
            /*
            {
                //crash                
                PhysicsServices.ReplicateBox2D(world, world);
                var b2world = world.GetUnique<PhysicsWorldComponent>().WorldReference;
                Box2DPhysics.SetBeginContactCallback(b2world, _cbkBeginContactDelegate);
                Box2DPhysics.SetEndContactCallback(b2world, _cbkEndContactDelegate);
                Box2DPhysics.SetPostSolveCallback(b2world, _cbkPostSolveDelegate);
                Box2DPhysics.SetPreSolveCallback(b2world, _cbkPreSolveDelegate);
            }*/

            var filter = world
                .Filter<Box2DBodyComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<Rotation2DComponent>()
                .End();
            
            var bodyReferenceComponent = world.GetPool<Box2DBodyComponent>();
            var poolRigidBodyComponent = world.GetPool<Box2DRigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<Rotation2DComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).BodyReference;
                var bodyInfo = Box2DApi.GetBodyInfo(bodyReference);
                
                ref var rigidBodyComponent = ref poolRigidBodyComponent.GetRef(entity);
                if (rigidBodyComponent.BodyType == BodyType.Dynamic)
                {
                    ref var positionComponent = ref poolPosition.GetRef(entity);
                    positionComponent.value.x = bodyInfo.Position.x;
                    positionComponent.value.z = bodyInfo.Position.y;
                    
                    rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
                    rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;
                    
                    poolRotation.GetRef(entity).Angle = bodyInfo.Angle;
                }
            }
        }

        private void UpdateInternalBox2DObjects()
        {
            Profiler.BeginSample("UpdateInternalBox2D");
            
            var filter = world
                .Filter<Box2DBodyComponent>()
                .Inc<Box2DRigidbodyComponent>()
                .Inc<PositionComponent>()
                .End();
            var poolBodyReference = world.GetPool<Box2DBodyComponent>();
            var poolRigidbody = world.GetPool<Box2DRigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<Rotation2DComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = poolBodyReference.Get(entity).BodyReference;
                var positionComponent = poolPosition.Get(entity);
                var rotationComponent = poolRotation.Get(entity);
                ref var rigidBodyComponent = ref poolRigidbody.GetRef(entity);

                var bodyInfo = new BodyInfo
                {
                    LinearVelocity = rigidBodyComponent.LinearVelocity,
                    AngularVelocity = rigidBodyComponent.AngularVelocity,
                    Angle = rotationComponent.Angle,
                    Awake = true
                };
                bodyInfo.Position.x = positionComponent.value.x;
                bodyInfo.Position.y = positionComponent.value.z;
                
                Box2DApi.SetBodyInfo(bodyReference, bodyInfo);
            }
        }

        private void UpdateBox2D()
        {
            var physicsWorld = world.GetUnique<Box2DWorldComponent>();
            var deltaTime = world.GetDeltaSeconds();

            Box2DApi.UpdateWorld(
                physicsWorld.WorldReference, 
                deltaTime, positionIterations, velocityIterations);
        }
        
        public void Run(EcsSystems systems)
        {
            Profiler.BeginSample("Box2DSystemRun");
            //create new bodies
            if (createBodies)
                CreateBodies();
            
            //write ecs components -> body
            if (updateInternalObjects)
                UpdateInternalBox2DObjects();
            
            //sim box2d world
            if (updateBox2d)
                UpdateBox2D();
            
            //write body -> ecs components
            if (writeToComponents)
                UpdateEcsComponents();
            Profiler.EndSample();
        }


        private static IntPtr CreateSimpleShape(EcsWorld world, int entity)
        {
            var boxCollider = entity.EntityGetNullable<Box2DBoxColliderComponent>(world);
            if (boxCollider.HasValue)
                return Box2DApi.CreateBoxShape(boxCollider.Value.Size / 2f);

            var circleCollider = entity.EntityGetNullable<Box2DCircleColliderComponent>(world);
            
            if (circleCollider.HasValue)
                return Box2DApi.CreateCircleShape(circleCollider.Value.Radius);
            
            var chainCollider = entity.EntityGetNullable<Box2DChainColliderComponent>(world);
            
            if (chainCollider.HasValue)
            {
                var vertices = chainCollider.Value.Points;
                return Box2DApi.CreateChainShape(vertices, vertices.Length);
            }

            return default;
        }
        private void CreateBodies()
        {
            if(!world.HasUnique<Box2DWorldComponent>()) 
                return;
            
            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;
            var filter = world
                .Filter<Box2DRigidbodyDefinitionComponent>()
                .Inc<Rotation2DComponent>()
                .Exc<Box2DBodyCreatedComponent>()
                .End();

            var poolRigidbodyDefinition = world.GetPool<Box2DRigidbodyDefinitionComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<Rotation2DComponent>();
            
            var poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
            var poolBodyReference = world.GetPool<Box2DBodyComponent>();
            
            var poolJoint = world.GetPool<JointTestComponent>();
            
            var poolBodyCreated = world.GetPool<Box2DBodyCreatedComponent>();
            var poolJointCreated = world.GetPool<Box2DJointCreatedComponent>();

            foreach (var entity in filter)
            {
                var def = poolRigidbodyDefinition.Get(entity);

                var positionComponent = poolPositionComponent.Get(entity);
                var bodyAngle = poolRotationComponent.Get(entity).Angle;
            
                var bodyReference = Box2DApi.CreateBody(
                    physicsWorld,
                    def.BodyType,
                    new Vector2(positionComponent.value.x, positionComponent.value.z),
                    bodyAngle,
                    entity);


                poolBodyCreated.Add(entity);
                
                
                var polygonCollider = entity.EntityGetNullable<Box2DPolygonColliderComponent>(world);
                if (polygonCollider.HasValue)
                {
                    var vertices = polygonCollider.Value.Vertices;
                    var anchors = polygonCollider.Value.Anchors;

                    int index = 0;
                    
                    foreach (var anchor in anchors)
                    {
                        shapeVertices.Clear();
                        for (int offset = index; index <= anchor + offset; index++)
                            shapeVertices.Add(vertices[index]);
                        var shape = Box2DApi.CreatePolygonShape(shapeVertices.ToArray(), shapeVertices.Count);
                        AddFixtureToBody(bodyReference, shape, def);
                    }
                }
                else
                {
                    var shape = CreateSimpleShape(world, entity);
                    AddFixtureToBody(bodyReference, shape, def);
                }

                Box2DApi.SetLinearDamping(bodyReference, def.LinearDamping);
                Box2DApi.SetAngularDamping(bodyReference, def.AngularDamping);
                
                poolBodyReference.Add(entity).BodyReference = bodyReference;
                poolRigidBody.Replace(entity).BodyType = def.BodyType;
            }

            filter = world.Filter<JointTestComponent>().Exc<Box2DJointCreatedComponent>().End();
            foreach (var entity in filter)
            {
                var joint = poolJoint.Get(entity);
                Box2DApi.b2DistanceJointDef def = Box2DApi.b2DistanceJointDef.Null;
                def.baseClass.bodyA = poolBodyReference.Get(entity).BodyReference;
                def.baseClass.bodyB = poolBodyReference.Get(joint.Entity).BodyReference;
                def.baseClass.collideConnected = false;
                def.minLength = 3;
                def.maxLength = 3;
                
                Box2DApi.CreateJoint(physicsWorld, def);
                poolJointCreated.Add(entity);
            }
        }

        private void AddFixtureToBody(IntPtr bodyReference, IntPtr shape,
            Box2DRigidbodyDefinitionComponent box2DRigidbodyDefinitionComponent)
        {
            B2Filter filter;
            filter.CategoryBits = box2DRigidbodyDefinitionComponent.CategoryBits;
            filter.MaskBits = box2DRigidbodyDefinitionComponent.MaskBits;
            filter.GroupIndex = box2DRigidbodyDefinitionComponent.GroupIndex;
            
            Box2DApi.AddFixtureToBody(bodyReference, shape,
                box2DRigidbodyDefinitionComponent.Density,
                box2DRigidbodyDefinitionComponent.Friction,
                box2DRigidbodyDefinitionComponent.Restitution,
                box2DRigidbodyDefinitionComponent.RestitutionThreshold,
                box2DRigidbodyDefinitionComponent.IsTrigger,
                filter);
        }

        public void Destroy(EcsSystems systems)
        {
            var box2d = world.GetUnique<Box2DWorldComponent>();
            Box2DApi.DestroyWorld(box2d.WorldReference);
            world.DelUnique<Box2DWorldComponent>();

            var poolRefs = world.GetPool<Box2DBodyComponent>();
            var poolCreated = world.GetPool<Box2DBodyComponent>();
            
            poolRefs.GetEntities().ForEach(entity =>
            {
                poolRefs.Del(entity);
            });
            
            poolCreated.GetEntities().ForEach(entity =>
            {
                poolCreated.Del(entity);
            });
        }
    }
}
