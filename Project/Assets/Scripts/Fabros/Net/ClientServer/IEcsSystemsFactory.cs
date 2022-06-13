using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public interface IEcsSystemsFactory
    {
        public struct Settings
        {
            public bool client;
            public bool server;
        }

        void AddNewSystems(EcsSystems systems, Settings settings);
    }
}