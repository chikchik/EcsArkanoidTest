using System;
using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class PopulatePhysicsWorldSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            if(!world.HasUnique<PhysicsWorldComponent>()) return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>();
            var filter = world
                .Filter<RigidbodyDefinitionComponent>()
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
                var bodyReference = CreateBody(physicsWorld, rigidbodyDefinitionComponent,
                    positionComponent, bodyAngle, entity);
                
                IntPtr shape = IntPtr.Zero;
                if (poolBoxCollider.Has(entity))
                {
                    shape = Box2DPhysics.CreateBoxShape(poolBoxCollider.Get(entity).size / 2f);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                    Box2DPhysics.SetAngularDamping(bodyReference, rigidbodyDefinitionComponent.angularDamping);
                }
                else if (poolCircleCollider.Has(entity))
                {
                    shape = Box2DPhysics.CreateCircleShape(poolCircleCollider.Get(entity).radius);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                }
                else if (poolPolygonCollider.Has(entity))
                {
                    var vertices = poolPolygonCollider.Get(entity).vertices;
                    var anchors = poolPolygonCollider.Get(entity).anchors;

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
                    var vertices = poolChainCollider.Get(entity).points;
                    
                    shape = Box2DPhysics.CreateChainShape(vertices, vertices.Length);
                    AddFixtureToBody(bodyReference, shape, rigidbodyDefinitionComponent);
                }
                
                Box2DPhysics.SetLinearDamping(bodyReference, rigidbodyDefinitionComponent.linearDamping);
                AddNewEntities(entity, world, bodyReference);
            }
        }

        private void AddNewEntities(int entity, EcsWorld world, IntPtr bodyReference)
        {
            ref var bodyReferenceComponent = ref entity.EntityAddComponent<BodyReferenceComponent>(world);
            bodyReferenceComponent.bodyReference = bodyReference;

            ref var rigidbodyComponent = ref entity.EntityReplace<RigidbodyComponent>(world);
        }

        private IntPtr CreateBody(PhysicsWorldComponent physicsWorld, RigidbodyDefinitionComponent rigidbodyDefinitionComponent,
            PositionComponent positionComponent, float angle, int entity)
        {
            var bodyReference = Box2DPhysics.CreateBody(
                physicsWorld.worldReference,
                (int) rigidbodyDefinitionComponent.bodyType,
                new Vector2(positionComponent.value.x, positionComponent.value.z),
                angle,
                entity);
            return bodyReference;
        }

        private void AddFixtureToBody(IntPtr bodyReference, IntPtr shape,
            RigidbodyDefinitionComponent rigidbodyDefinitionComponent, bool isTrigger = false)
        {
            if (shape == IntPtr.Zero) return;
            Box2DPhysics.AddFixtureToBody(bodyReference, shape,
                rigidbodyDefinitionComponent.density,
                rigidbodyDefinitionComponent.friction,
                rigidbodyDefinitionComponent.restitution,
                rigidbodyDefinitionComponent.restitutionThreshold,
                isTrigger);
        }
    }
}
