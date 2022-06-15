using System;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Serializer;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    [Serializable]
    public class WorldUpdateProto
    {
        public WorldDiff difStr;
        public string difBinary;
        public int delay;
    }
}