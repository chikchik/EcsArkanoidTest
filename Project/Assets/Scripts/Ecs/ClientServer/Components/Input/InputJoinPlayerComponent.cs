using System;

namespace Game.Ecs.ClientServer.Components.Input
{
    [Serializable]
    public struct InputJoinPlayerComponent
    {
        public bool leave;
        public int playerID;
    }
}