using System;
using Game.Fabros.Net.ClientServer.Protocol;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct PendingInputComponent
    {
        public UserInput[] data;
    }
}