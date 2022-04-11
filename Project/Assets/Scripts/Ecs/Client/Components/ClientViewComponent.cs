using UnityEngine;

namespace Game.Ecs.Client.Components
{
    public struct ClientViewComponent
    {
        public Global Global;
        public UI.MainUI MainUI;
        public Camera Camera;
    }
}