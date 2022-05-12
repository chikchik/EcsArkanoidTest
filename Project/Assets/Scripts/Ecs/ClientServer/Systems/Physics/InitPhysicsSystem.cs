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
            {
                ref var physicsWorldComponent = ref _ecsWorld.AddUnique<PhysicsWorldComponent>();
                physicsWorldComponent.worldReference = Box2DPhysics.CreateWorld(Config.GRAVITY);
                
                _cbkBeginContactDelegate = SetBeginContactCallback;
                _cbkEndContactDelegate = SetEndContactCallback;
                _cbkPostSolveDelegate = SetPreSolveCallback;
                _cbkPreSolveDelegate = SetPostSolveCallback;
                
                Box2DPhysics.SetBeginContactCallback(physicsWorldComponent.worldReference, _cbkBeginContactDelegate);
                Box2DPhysics.SetEndContactCallback(physicsWorldComponent.worldReference, _cbkEndContactDelegate);
                Box2DPhysics.SetPostSolveCallback(physicsWorldComponent.worldReference, _cbkPostSolveDelegate);
                Box2DPhysics.SetPreSolveCallback(physicsWorldComponent.worldReference, _cbkPreSolveDelegate);
            }
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
