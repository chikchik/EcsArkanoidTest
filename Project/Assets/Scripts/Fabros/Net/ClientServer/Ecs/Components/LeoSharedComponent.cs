using Fabros.Ecs;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Components
{
    public struct LeoSharedComponent
    {
        public SyncLog Log;

        public LeoContexts Context;
        public ComponentsPool Pool;
    }
}