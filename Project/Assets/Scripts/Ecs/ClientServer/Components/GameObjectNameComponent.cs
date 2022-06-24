using System;
using Fabros.Ecs.ClientServer.WorldDiff;

namespace Game.Ecs.ClientServer.Components
{
    [ForceJsonSerialize]
    [Serializable]
    public struct GameObjectNameComponent
    {
        public string Name;
    }
}