using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct LeoConfigComponent
    {
        public int serverTickrate;
        public int clientTickrate;

        public int serverSyncStep;
        //public int serverTicksPerClient;
    }
}