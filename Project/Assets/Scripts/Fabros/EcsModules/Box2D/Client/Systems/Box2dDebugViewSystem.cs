using System;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.Client.Systems
{
    public class Box2dDebugViewSystem : IEcsInitSystem, IEcsDestroySystem
    {
        private Box2dGizmosView _gizmosView;

        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<PhysicsWorldComponent>()) 
                return;
            
            var physicsWorld = world.GetUnique<PhysicsWorldComponent>().WorldReference;
            _gizmosView = GameObject.FindObjectOfType<Box2dGizmosView>();

            if (!_gizmosView)
            {
                var go = new GameObject("[Box2DGizmosView]");
                _gizmosView = go.AddComponent<Box2dGizmosView>();
            }
            _gizmosView.SetBox2D(physicsWorld);
            
            var box2dDebugDrawFlags = (UInt32) (Box2dDebugDrawFlags.ShapeBit | Box2dDebugDrawFlags.JointBit |
                                                Box2dDebugDrawFlags.AabbBit | Box2dDebugDrawFlags.PairBit |
                                                Box2dDebugDrawFlags.CenterOfMassBit | Box2dDebugDrawFlags.ContactBit);
            Box2DApi.SetFlagsForDebugDraw(physicsWorld, box2dDebugDrawFlags);
        }
        
        public void Destroy(EcsSystems systems)
        {
            _gizmosView.SetBox2D(default);
        }
    }
}