using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEditor.Experimental;
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
        public static IntPtr ClonePhysicsWorldAndChangeEcsBodyRef(EcsWorld world, IntPtr physicsWorld)
        {
            
            EcsFilter filter = world.Filter<BodyReferenceComponent>().End();
            var entities = filter.GetEntities();
            var poolBodyReferences = world.GetPool<BodyReferenceComponent>();
            BodyReferenceComponent[] arrayOfReferences = new BodyReferenceComponent[entities.Count];

            for (int i = 0; i < entities.Count; i++)
            {
                ref var bodyRef = ref poolBodyReferences.GetRef(entities[i]);
                // ref IntPtr ptrRef = ref bodyRef.BodyReference;
                arrayOfReferences[i] = bodyRef;
                // Debug.Log($"Reference before {arrayOfReferences[i].BodyReference} {bodyRef.BodyReference}");
            }
            
            var cloneWorld = Box2DPhysics.CloneWorld(ref arrayOfReferences, entities.Count, physicsWorld);

            ref var worldReference = ref world.GetUniqueRef<PhysicsWorldComponent>().WorldReference;
            Box2DPhysics.DestroyWorld(worldReference);

            

            for (int i = 0; i < entities.Count; i++)
            {
                ref var bodyRef = ref poolBodyReferences.GetRef(entities[i]);
                bodyRef.BodyReference = arrayOfReferences[i].BodyReference;
                // Debug.Log($"Reference after {arrayOfReferences[i].BodyReference} {bodyRef.BodyReference}");
            }
            worldReference = cloneWorld;

            return cloneWorld;
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