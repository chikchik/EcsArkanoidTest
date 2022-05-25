using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.ClientServer.Physics;
using Game.ClientServer.Physics.Components;
using Game.Ecs.ClientServer.Components.Physics;
using Leopotam.EcsLite;
using UnityEngine;
using Object = System.Object;

namespace Game.Ecs.ClientServer.Systems.Physics
{
    public class InitPhysicsSystem : IEcsInitSystem 
    {
        private EcsWorld _ecsWorld;
        private Box2DPhysics.CallbackDelegate _cbkBeginContactDelegate;
        private Box2DPhysics.CallbackDelegate _cbkEndContactDelegate;
        private Box2DPhysics.CallbackDelegate _cbkPostSolveDelegate;
        private Box2DPhysics.CallbackDelegate _cbkPreSolveDelegate;
        
        public void Init(EcsSystems systems)
        {
            _ecsWorld = systems.GetWorld();
            if (!_ecsWorld.HasUnique<PhysicsWorldComponent>())
                _ecsWorld.AddUnique<PhysicsWorldComponent>().WorldReference = Box2DPhysics.CreateWorld(Config.GRAVITY);

            var b2world = _ecsWorld.GetUnique<PhysicsWorldComponent>().WorldReference;

            _cbkBeginContactDelegate = SetBeginContactCallback;
            _cbkEndContactDelegate = SetEndContactCallback;
            _cbkPostSolveDelegate = SetPreSolveCallback;
            _cbkPreSolveDelegate = SetPostSolveCallback;
            
            Box2DPhysics.SetBeginContactCallback(b2world, _cbkBeginContactDelegate);
            Box2DPhysics.SetEndContactCallback(b2world, _cbkEndContactDelegate);
            Box2DPhysics.SetPostSolveCallback(b2world, _cbkPostSolveDelegate);
            Box2DPhysics.SetPreSolveCallback(b2world, _cbkPreSolveDelegate);
            
        }
        
        public void SetBeginContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<BeginContactComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetEndContactCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<EndContactComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPreSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PreSolveComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
        
        public void SetPostSolveCallback(CollisionCallbackData callbackData)
        {
            var newEntity = _ecsWorld.NewEntity();
            ref var contact = ref newEntity.EntityAddComponent<PostSolveComponent>(_ecsWorld);
            contact.CollisionCallbackData = callbackData;
        }
    }
}
