using Gaming.ContainerManager.ImageContracts.V1;
using XFlow.EcsLite;

namespace XFlow.Server
{
    public struct ClientComponent
    {
        //public int ID;
        
        public int Delay;
        public int DelayMs;
        
        
        public int LastPingTick;
        public int LastClientTick;
        public int LastServerTick;
        public string UserId;
        public IUserAddress ReliableAddress;
        public IUserAddress UnreliableAddress;

        public EcsWorld SentWorld;
        public EcsWorld SentWorldReliable;
    }
}