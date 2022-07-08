using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Client
{
    public class Game : EventsSystem<WeaponComponent>.IAnyComponentChangedListener
    {
        public void OnAnyComponentChanged(EcsWorld world, int entity, WeaponComponent data, bool added)
        {
            throw new System.NotImplementedException();
        }
    }
}