using System;
using System.Collections.Generic;
using Game.ClientServer.Physics;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
{
    public class Box2dGizmosView : MonoBehaviour
    {
        [Inject] private EcsWorld _ecsWorld = null;
        private IntPtr _physicsWorldRef = IntPtr.Zero;
        
        private void OnDrawGizmos()
        {
            if (_ecsWorld != null && !_ecsWorld.HasUnique<PhysicsWorldComponent>()) return;
            if (_ecsWorld != null && _physicsWorldRef == IntPtr.Zero)
                _physicsWorldRef = _ecsWorld.GetUnique<PhysicsWorldComponent>().WorldReference;
            
            Box2DPhysics.DebugDraw(_physicsWorldRef);
        }
    }
}