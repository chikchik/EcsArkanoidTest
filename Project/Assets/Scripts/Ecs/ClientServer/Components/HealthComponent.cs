using System;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct HealthComponent
    {
        public float maxHealth;
        public float health;
    }
}