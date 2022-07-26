using Flow.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public interface IEcsSystemsFactory
    {
        public struct Settings
        {
            public bool AddClientSystems;
            public bool AddServerSystems;
        }

        void AddNewSystems(EcsSystems systems, Settings settings);

        IEcsSystem CreateSyncDebugSystem(bool pre);
    }
}