using System.Net;
using XFlow.EcsLite;
using XFlow.P2P;

namespace XFlow.Server
{
    struct ClientComponent
    {
        public int ID;
        
        public int Delay;
        public int DelayMs;
        
        
        public int LastPingTick;
        public int LastClientTick;
        public int LastServerTick;
        public ClientAddr Address;
        public EndPoint EndPoint;

        public EcsWorld SentWorld;
        public EcsWorld SentWorldRelaible;
    }
}