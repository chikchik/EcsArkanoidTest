using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public interface IInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, int tick, IInputComponent input);
    }
}