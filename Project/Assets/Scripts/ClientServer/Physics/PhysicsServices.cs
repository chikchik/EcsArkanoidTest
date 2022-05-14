using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;

namespace Game.ClientServer.Physics
{
    public static class PhysicsServices
    {
#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
        private const string DllName = "libbox2d";
#endif
        

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