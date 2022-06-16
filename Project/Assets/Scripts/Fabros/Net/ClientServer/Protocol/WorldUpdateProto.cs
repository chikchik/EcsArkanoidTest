using System;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Serializer;

namespace Game.Fabros.Net.ClientServer.Protocol
{
    [Serializable]
    public class WorldUpdateProto
    {
        public string difStr;
        public string difBinary;
        public int delay;
    }
}