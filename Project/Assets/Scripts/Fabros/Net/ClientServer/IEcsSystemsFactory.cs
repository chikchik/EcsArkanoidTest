using Fabros.Ecs.ClientServer.Serializer;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public interface IEcsSystemsFactory
    {
        public struct Settings
        {
            public bool client;
            public bool server;

            public Settings(bool client, bool server)
            {
                this.client = client;
                this.server = server;
            }
        }

        void AddNewSystems(EcsSystems systems, Settings settings);
    }
}