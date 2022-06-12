using Fabros.EcsModules.Tick.Other;
using Game.Fabros.Net.ClientServer.Ecs.Components;

namespace Game.Fabros.Net.ClientServer
{
    public struct UserInput
    {
        public Tick Tick { get; set; }
        public int PlayerID;
        public IInputComponent Component;
    }
}