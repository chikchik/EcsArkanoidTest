using System;
using Fabros.Ecs.ClientServer.WorldDiff;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    public struct MoveInfoComponent
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
    }
}