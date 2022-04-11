using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct InteractableComponent
    {
        public bool isInteractable;
        public bool canInteract;
    }
}