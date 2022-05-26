using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.ClientServer.Box2D;
using Game.ClientServer.Box2D.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;
using Object = System.Object;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class Box2DSystem : IEcsInitSystem, IEcsRunSystem, IEcsDestroySystem, IEcsWorldChangedSystem
    {
        private Box2DApi.CallbackDelegate _cbkBeginContactDelegate;
        private Box2DApi.CallbackDelegate _cbkEndContactDelegate;
        private Box2DApi.CallbackDelegate _cbkPostSolveDelegate;
        private Box2DApi.CallbackDelegate _cbkPreSolveDelegate;
        
        private EcsWorld world;
        
        private List<Vector2> shapeVertices = new List<Vector2>();
        
        public void Init(EcsSystems systems)
        {
            world = systems.GetWorld();
            if (!world.HasUnique<PhysicsWorldComponent>())
                world.AddUnique<PhysicsWorldComponent>().WorldReference = Box2DApi.CreateWorld(Config.GRAVITY);

            var b2world = world.GetUnique<PhysicsWorldComponent>().WorldReference;
            if (b2world == default)
            {
                throw new Exception("b2world is null");
            }

            _cbkBeginContactDelegate = SetBeginContactCallback;
            _cbkEndContactDelegate = SetEndContactCallback;
            _cbkPostSolveDelegate = SetPreSolveCallback;
            _cbkPreSolveDelegate = SetPostSolveCallback;
            
            Box2DApi.SetBeginContactCallback(b2world, _cbkBeginContactDelegate);
            Box2DApi.SetEndContactCallback(b2world, _cbkEndContactDelegate);
            Box2DApi.SetPostSolveCallback(b2world, _cbkPostSolveDelegate);
            Box2DApi.SetPreSolveCallback(b2world, _cbkPreSolveDelegate);
        }
        
        public void WorldChanged(EcsWorld world)
        {
            if (this.world != world)
                throw new NotImplementedException("WorldChanged not supported");
        }
        
        public void SetBeginContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<BeginContactComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetEndContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<EndContactComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPreSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PreSolveComponent>(world);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPostSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = world.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PostSolveComponent>(world);
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
                .Filter<BodyReferenceComponent>()
                .Inc<RigidbodyComponent>()
                .Inc<PositionComponent>()
                .Inc<RotationComponent>()
                .End();
            
            var bodyReferenceComponent = world.GetPool<BodyReferenceComponent>();
            var poolRigidBodyComponent = world.GetPool<RigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<RotationComponent>(); 
            
            foreach (var entity in filter)
            {
                var bodyReference = bodyReferenceComponent.Get(entity).BodyReference;
                var bodyInfo = Box2DApi.GetBodyInfo(bodyReference);
                
                ref var rigidBodyComponent = ref poolRigidBodyComponent.GetRef(entity);
                if (rigidBodyComponent.BodyType == BodyType.Dynamic)
                {
                    ref var positionComponent = ref poolPosition.GetRef(entity);
                    ref var rotationComponent = ref poolRotation.GetRef(entity);

                    positionComponent.value.x = bodyInfo.Position.x;
                    positionComponent.value.z = bodyInfo.Position.y;
                    rigidBodyComponent.LinearVelocity = bodyInfo.LinearVelocity;
                    rigidBodyComponent.AngularVelocity = bodyInfo.AngularVelocity;
                    rotationComponent.value = bodyInfo.Angle;
                }
            }
        }

        private void UpdateInternalBox2D()
        {
            var filter = world
                .Filter<BodyReferenceComponent>()
                .Inc<RigidbodyComponent>()
                .Inc<PositionComponent>()
                .End();
            var poolBodyReference = world.GetPool<BodyReferenceComponent>();
            var poolRigidbody = world.GetPool<RigidbodyComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRotation = world.GetPool<RotationComponent>(); 
            
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
                    Angle = rotationComponent.value
                };
                bodyInfo.Position.x = positionComponent.value.x;
                bodyInfo.Position.y = positionComponent.value.z;
                
                Box2DApi.SetBodyInfo(bodyReference, bodyInfo);
            }
        }

        private void UpdateBox2D()
        {
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var deltaTime = world.GetDeltaSeconds();

            Box2DApi.UpdateWorld(
                physicsWorld.WorldReference, 
                deltaTime, 
                Config.POSITION_ITERATIONS, 
                Config.VELOCITY_ITERATIONS);
        }
        
        public void Run(EcsSystems systems)
        {
            //create new bodies
            CreateBodies();
            
            //write ecs components -> body
            UpdateInternalBox2D();
            
            //sim box2d world
            UpdateBox2D();
            
            //write body -> ecs components
            UpdateEcsComponents();
        }


        private static IntPtr CreateSimpleShape(EcsWorld world, int entity)
        {
            var boxCollider = entity.EntityGetNullable<BoxColliderComponent>(world);
            if (boxCollider.HasValue)
                return Box2DApi.CreateBoxShape(boxCollider.Value.Size / 2f);

            var circleCollider = entity.EntityGetNullable<CircleColliderComponent>(world);
            
            if (circleCollider.HasValue)
                return Box2DApi.CreateCircleShape(circleCollider.Value.Radius);
            
            var chainCollider = entity.EntityGetNullable<ChainColliderComponent>(world);
            
            if (chainCollider.HasValue)
            {
                var vertices = chainCollider.Value.Points;
                return Box2DApi.CreateChainShape(vertices, vertices.Length);
            }

            return default;
        }
        private void CreateBodies()
        {
            if(!world.HasUnique<PhysicsWorldComponent>()) 
                return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>().WorldReference;
            var filter = world
                .Filter<RigidbodyDefinitionComponent>()
                .Inc<RotationComponent>()
                .Exc<BodyCreatedComponent>()
                .End();

            var poolRigidbodyDefinition = world.GetPool<RigidbodyDefinitionComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<RotationComponent>();
            
            var poolBodyCreated = world.GetPool<BodyCreatedComponent>();
            var poolRigidBody = world.GetPool<RigidbodyComponent>();
            var poolBodyReference = world.GetPool<BodyReferenceComponent>();

            foreach (var entity in filter)
            {
                var def = poolRigidbodyDefinition.Get(entity);

                var positionComponent = poolPositionComponent.Get(entity);
                var bodyAngle = poolRotationComponent.Get(entity).value;
            
                var bodyReference = Box2DApi.CreateBody(
                    physicsWorld,
                    (int) def.BodyType,
                    new Vector2(positionComponent.value.x, positionComponent.value.z),
                    bodyAngle,
                    entity);


                poolBodyCreated.Add(entity);
                
                
                var polygonCollider = entity.EntityGetNullable<PolygonColliderComponent>(world);
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
                poolRigidBody.Add(entity).BodyType = def.BodyType;
            }
        }

        private void AddFixtureToBody(IntPtr bodyReference, IntPtr shape,
            RigidbodyDefinitionComponent rigidbodyDefinitionComponent)
        {
            B2Filter filter;
            filter.CategoryBits = rigidbodyDefinitionComponent.CategoryBits;
            filter.MaskBits = rigidbodyDefinitionComponent.MaskBits;
            filter.GroupIndex = rigidbodyDefinitionComponent.GroupIndex;
            
            Box2DApi.AddFixtureToBody(bodyReference, shape,
                rigidbodyDefinitionComponent.Density,
                rigidbodyDefinitionComponent.Friction,
                rigidbodyDefinitionComponent.Restitution,
                rigidbodyDefinitionComponent.RestitutionThreshold,
                rigidbodyDefinitionComponent.IsTrigger,
                filter);
        }

        public void Destroy(EcsSystems systems)
        {
            var box2d = world.GetUnique<PhysicsWorldComponent>();
            Box2DApi.DestroyWorld(box2d.WorldReference);
            world.DelUnique<PhysicsWorldComponent>();

            var poolRefs = world.GetPool<BodyReferenceComponent>();
            var poolCreated = world.GetPool<BodyReferenceComponent>();
            
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
