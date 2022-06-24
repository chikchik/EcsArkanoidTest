using Fabros.Ecs.ClientServer.WorldDiff;
using System;

namespace Game.Ecs.ClientServer.Components.Objective
{
    [ForceJsonSerialize]
    [Serializable]
    public struct ObjectiveDescriptionComponent
    {
        public string text;
    }
}