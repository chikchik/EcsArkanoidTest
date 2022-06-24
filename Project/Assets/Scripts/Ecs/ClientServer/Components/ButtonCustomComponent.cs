using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonCustomComponent
    {
        public bool Spawn;
        public bool Shake;
    }
}