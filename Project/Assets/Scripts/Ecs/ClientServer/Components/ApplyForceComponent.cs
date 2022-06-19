using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ApplyForceComponent
    {
        public float Time;
        public Vector3 Direction;
    }
    
    [Serializable]
    public struct ShootingComponent
    {
        public float ShootAtTime;
        public Vector3 Direction;
        public float TotalTime;
        public bool ShootMade;
    }
    
    [EmptyComponent]
    [Serializable]
    public struct ShootStartedComponent
    {
    }
    
    [EmptyComponent]
    [Serializable]
    public struct CantMoveComponent
    {
        
    }
}