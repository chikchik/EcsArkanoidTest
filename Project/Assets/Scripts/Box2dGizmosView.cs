using System;
using System.Collections.Generic;
using Game.Fabros.EcsModules.Box2D.ClientServer.Api;
using Game.Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
{
    //todo convert to EcsSystem
    public class Box2dGizmosView : MonoBehaviour
    {
        [Inject] private EcsWorld _ecsWorld = null;
        private IntPtr _physicsWorldRef = IntPtr.Zero;

        [SerializeField] private bool show = false;
        
        private void OnDrawGizmos()
        {
            if (!show) return;
            if (_ecsWorld != null && !_ecsWorld.HasUnique<PhysicsWorldComponent>()) return;
            if (_ecsWorld != null && _physicsWorldRef == IntPtr.Zero)
                _physicsWorldRef = _ecsWorld.GetUnique<PhysicsWorldComponent>().WorldReference;
            
            Box2DApi.DebugDraw(_physicsWorldRef);
        }
    }
}