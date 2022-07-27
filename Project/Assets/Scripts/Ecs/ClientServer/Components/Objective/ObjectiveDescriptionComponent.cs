using System;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;

namespace Game.Ecs.ClientServer.Components.Objective
{
    [ForceJsonSerialize]
    [Serializable]
    public struct ObjectiveDescriptionComponent
    {
        public string text;
    }
}