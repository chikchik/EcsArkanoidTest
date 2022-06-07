using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Colliders;
using Leopotam.EcsLite;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.ClientServer
{
    public static class Box2DServices
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif


#if CLIENT
        public static void PrintClassOffsets<T>()
        {
            string str = String.Empty;
            str += "Offsets:\n";
            foreach (var fieldInfo in typeof(T).GetFields())
            {
                str += $"{fieldInfo.Name} = {Marshal.OffsetOf<T>(fieldInfo.Name)}\n";
            }
            Debug.Log(str);

            // Debug.Log($"{Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("type")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("userData")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("bodyA")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("bodyB")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("collideConnected")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("localAnchorA")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("localAnchorB")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("length")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("minLength")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("maxLength")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("stiffness")}, {Marshal.OffsetOf<Box2DApi.b2DistanceJointDef>("damping")} ");
        }
#endif

        // Clone current physicsWorld, delete old, change all old body references to the new ones, return cloned world
        public static void ReplicateBox2D(EcsWorld src, EcsWorld dest)
        {
            var srcEntities = new List<int>();
            src.GetPool<Box2DBodyComponent>().GetEntities(srcEntities);
            var srcPool = src.GetPool<Box2DBodyComponent>();
            
            IntPtr[] arrayOfReferences = new IntPtr[srcEntities.Count + 1];
            for (int i = 0; i < srcEntities.Count; i++)
                arrayOfReferences[i] = srcPool.Get(srcEntities[i]).BodyReference;

            var srcWorld = src.GetUnique<Box2DWorldComponent>().WorldReference;
            if (srcWorld == default)
                return;
            var newDestWorld = Box2DApi.CloneWorld(ref arrayOfReferences, srcEntities.Count, srcWorld);

            
            var destPool = dest.GetPool<Box2DBodyComponent>();
            var destPoolBodyCreated = dest.GetPool<Box2DBodyCreatedComponent>();
            
            
            if (dest.HasUnique<Box2DWorldComponent>())
            {
                var oldDestWorld = dest.ReplaceUnique<Box2DWorldComponent>().WorldReference;
                if (oldDestWorld != default)
                    Box2DApi.DestroyWorld(oldDestWorld);
                
                var destEntities = new List<int>();
                destPool.GetEntities(destEntities);

                for (int i = 0; i < destEntities.Count; i++)
                {
                    var entity = destEntities[i];
                    destPool.Del(entity);
                    destPoolBodyCreated.Del(entity);
                }
            }
            
            dest.ReplaceUnique<Box2DWorldComponent>().WorldReference = newDestWorld;

            for (int i = 0; i < srcEntities.Count; i++)
            {
                var entity = srcEntities[i];
                destPool.Replace(entity).BodyReference = arrayOfReferences[i];
                destPoolBodyCreated.Replace(entity);
            }
        }

        public static IntPtr CreateBodyNow(EcsWorld world, int entity, List<Vector2> shapeVertices = null)
        {
            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;
            var poolRigidbodyDefinition = world.GetPool<Box2DRigidbodyDefinitionComponent>();
            var poolBodyReference = world.GetPool<Box2DBodyComponent>();
            var poolPositionComponent = world.GetPool<PositionComponent>();
            var poolRotationComponent = world.GetPool<Rotation2DComponent>();
            var poolRigidBody = world.GetPool<Box2DRigidbodyComponent>();
            var poolBodyCreated = world.GetPool<Box2DBodyCreatedComponent>();
            
            var positionComponent = poolPositionComponent.Get(entity);
            var bodyAngle = poolRotationComponent.Get(entity).Angle;
            
            var def = poolRigidbodyDefinition.Get(entity);
            
            var bodyReference = Box2DApi.CreateBody(
                physicsWorld,
                def.BodyType,
                new Vector2(positionComponent.value.x, positionComponent.value.z),
                bodyAngle,
                entity);
            
            Box2DApi.SetBullet(bodyReference, def.Bullet);


            poolBodyCreated.Add(entity);
                
                
            var polygonCollider = entity.EntityGetNullable<Box2DPolygonColliderComponent>(world);
            if (polygonCollider.HasValue)
            {
                var vertices = polygonCollider.Value.Vertices;
                var anchors = polygonCollider.Value.Anchors;

                int index = 0;
                if (shapeVertices == null)
                    shapeVertices = new List<Vector2>();
                    
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
            return bodyReference;
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
        
        private static void AddFixtureToBody(IntPtr bodyReference, IntPtr shape, Box2DRigidbodyDefinitionComponent box2DRigidbodyDefinitionComponent)
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

        public static IntPtr GetBodyRefFromEntity(this EcsWorld world, int entity)
        {
            IntPtr bodyRef = IntPtr.Zero;

            var ecsPool = world.GetPool<Box2DBodyComponent>();

            if (ecsPool.Has(entity))
            {
                bodyRef = ecsPool.Get(entity).BodyReference;
            }
                        
            return bodyRef;
        }
        
        [DllImport(DllName)]
        public static extern int GetEntityFromBody(IntPtr body);

        [DllImport(DllName)]
        public static extern IntPtr GetWorldFromBody(IntPtr body);
    }
}