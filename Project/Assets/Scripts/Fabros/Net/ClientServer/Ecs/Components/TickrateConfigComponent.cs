using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct TickrateConfigComponent
    {
        public int serverTickrate;
        public int clientTickrate;

        public int serverSyncStep;
    }
}