using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct ButtonComponent
    {
        public bool isActivated;
    }

    [Serializable]
    public struct ButtonPushCompleted
    {
    }
}