using System;
using Fabros.Ecs.ClientServer.WorldDiff;
using UnityEngine;

namespace Game.Ecs.ClientServer.Components
{
    [DontSerialize]//как тест что оно может целиком на сервере определяться
    public struct MoveInfoComponent
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
    }
}