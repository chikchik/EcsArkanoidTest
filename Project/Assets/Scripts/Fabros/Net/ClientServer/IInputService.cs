using Game.Ecs.ClientServer.Components.Input;
using Leopotam.EcsLite;

namespace Game.Fabros.Net.ClientServer
{
    public interface IInputService
    {
        public void Input(EcsWorld inputWorld, int playerId, IInputComponent input);
    }
}