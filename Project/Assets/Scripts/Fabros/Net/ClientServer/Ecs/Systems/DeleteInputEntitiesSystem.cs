using Game.Ecs.ClientServer.Components.Input;
using Flow.EcsLite;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class DeleteInputEntitiesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var inputWorld = systems.GetWorld("input");

            var filter = inputWorld.Filter<InputComponent>()
                .Exc<InputTickComponent>().End();

            foreach (var inputEntity in filter)
            {
                inputWorld.DelEntity(inputEntity);
            }
        }
    }
}