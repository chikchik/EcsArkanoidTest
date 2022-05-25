using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.ClientServer.Physics
{
    public static class PhysicsServices
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif

        // Clone current physicsWorld, delete old, change all old body references to the new ones, return cloned world
        public static void ReplicateBox2D(EcsWorld src, EcsWorld dest)
        {
            var entities = new List<int>();
            src.GetPool<BodyReferenceComponent>().GetEntities(entities);
            var srcPool = src.GetPool<BodyReferenceComponent>();
            
            IntPtr[] arrayOfReferences = new IntPtr[entities.Count];
            for (int i = 0; i < entities.Count; i++)
                arrayOfReferences[i] = srcPool.Get(entities[i]).BodyReference;

            var srcWorld = src.GetUnique<PhysicsWorldComponent>().WorldReference;
            var newWorld = Box2DPhysics.CloneWorld(ref arrayOfReferences, entities.Count, srcWorld);

            dest.AddUnique<PhysicsWorldComponent>().WorldReference = newWorld;
            var destPool = dest.GetPool<BodyReferenceComponent>();
            var destPoolBodyCreated = dest.GetPool<BodyCreatedComponent>();

            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                destPool.Add(entity).BodyReference = arrayOfReferences[i];
                destPoolBodyCreated.Add(entity);
            }
        }

        public static IntPtr GetBodyRefFromEntity(this EcsWorld world, int entity)
        {
            IntPtr bodyRef = IntPtr.Zero;

            var ecsPool = world.GetPool<BodyReferenceComponent>();

            if (ecsPool.Has(entity))
            {
                bodyRef = ecsPool.Get(entity).BodyReference;
            }
                        
            return bodyRef;
        }
        
        [DllImport(DllName)]
        public static extern int GetEntityFromBody(IntPtr body);

        [DllImport(DllName)]
        public static extern IntPtr GetWorldFromBody(this IntPtr body);
    }
}