using System;
using Game.UI;
using Game.UI.Mono;

namespace Game.Ecs.Client.Components
{
    [Serializable]
    public struct HealthViewComponent
    {
        public HpView healthView;
    }
}