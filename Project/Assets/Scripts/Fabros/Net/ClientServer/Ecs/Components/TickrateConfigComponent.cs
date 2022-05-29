using System;

namespace Game.Fabros.Net.ClientServer.Ecs.Components
{
    [Serializable]
    public struct TickrateConfigComponent
    {
        public int serverTickrate;
        public int clientTickrate;

        public int serverSyncStep;
    }
}