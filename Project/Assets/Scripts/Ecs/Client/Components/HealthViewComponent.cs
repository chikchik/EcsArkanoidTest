using System;
using Game.UI;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct HealthViewComponent
    {
        public HpView healthView;
    }
}