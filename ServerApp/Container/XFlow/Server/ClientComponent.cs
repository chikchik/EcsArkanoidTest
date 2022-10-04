using System.Net;
using Gaming.ContainerManager.ImageContracts.V1;
using XFlow.EcsLite;
using XFlow.P2P;

namespace XFlow.Server
{
    struct ClientComponent
    {
        public int ID;
        public int Delay;
        public int LastPingTick;
        public int LastClientTick;
        public int LastServerTick;
        public EndPoint EndPoint;
        public IUserAddress UserAddress;

        public EcsWorld SentWorld;
        public EcsWorld SentWorldRelaible;
    }
}