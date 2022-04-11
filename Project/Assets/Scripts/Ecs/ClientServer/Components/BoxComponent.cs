using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct BoxComponent
    {
    }
    
    [Serializable]
    public struct OpenedBoxComponent
    {
        public bool isOpened;
    }
}