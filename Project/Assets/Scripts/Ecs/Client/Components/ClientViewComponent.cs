using Game.UI;
using UnityEngine;

namespace Game.Ecs.Client.Components
{
    public struct ClientViewComponent
    {
        public Global Global;
        public MainUI MainUI;
        public Camera Camera;
    }
}