using System;
using System.Runtime.InteropServices;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace Game.Ecs.ClientServer.Components
{
    [ForceJsonSerialize]
    [Serializable]
    public struct GameObjectNameComponent
    {
        public string Name;
    }
}