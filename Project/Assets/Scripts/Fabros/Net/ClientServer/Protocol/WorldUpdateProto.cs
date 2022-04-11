using System;
using Fabros.Ecs;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    [Serializable]
    public class WorldUpdateProto
    {
        public WorldDiff dif;
        public int delay;
    }
}