using System;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.Client.Systems
{
    public class Box2DDebugViewSystem : IEcsInitSystem, IEcsDestroySystem
    {
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<Box2DWorldComponent>()) 
                return;
            
            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;
            Box2DGizmosView.ReplaceBox2D(physicsWorld);
            
            var box2dDebugDrawFlags = (UInt32) (Box2DDebugDrawFlags.ShapeBit | Box2DDebugDrawFlags.JointBit |
                                                Box2DDebugDrawFlags.AabbBit | Box2DDebugDrawFlags.PairBit |
                                                Box2DDebugDrawFlags.CenterOfMassBit | Box2DDebugDrawFlags.ContactBit);
            Box2DApi.SetFlagsForDebugDraw(physicsWorld, box2dDebugDrawFlags);
        }
        
        public void Destroy(EcsSystems systems)
        {
            Box2DGizmosView.ReplaceBox2D(default);
        }


        public static void ReplaceBox2D(EcsWorld world)
        {
            var physicsWorld = world.GetUnique<Box2DWorldComponent>().WorldReference;
            Box2DGizmosView.ReplaceBox2D(physicsWorld);
        }
    }
}