using System;

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