using Game.Ecs.Client.Components;
using XFlow.EcsLite;

namespace Game.Client
{
    public class ResetMouseDownSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            systems.GetWorld().DelUnique<MouseDownHandledComponent>();
        }
    }
}