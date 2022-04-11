using System;
using Game.Fabros.Net.ClientServer.Protocol;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct LeoPendingInputComponent
    {
        public UserInput[] data;
    }
}