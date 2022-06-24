using System;

namespace Game.Fabros.Net.ClientServer.Ecs.Components
{
    [Serializable]
    public struct TickrateConfigComponent
    {
        public int Tickrate;
        public int ServerSyncStep;
    }
}