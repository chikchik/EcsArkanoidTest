using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Game.Ecs.Client.Components;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.EcsModules.Fire.Client.Systems
{
    public class FireViewSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var global = world.GetUnique<ClientViewComponent>().Global;
            var filter = world.Filter<FireComponent>().End();
            var poolFire = world.GetPool<FireComponent>();
            var poolFireView = world.GetPool<FireViewComponent>();
            var poolGO = world.GetPool<GameObjectComponent>();


            foreach (var entity in filter)
            {
                if (poolFireView.Has(entity))
                    continue;

                var fireComponent = poolFire.Get(entity);

                var ps = Object.Instantiate(global.FireParticles);
                ps.transform.position = entity.EntityGetComponent<PositionComponent>(world).value;
                ps.name = entity.ToString();
                ps.transform.localScale = Vector3.one * fireComponent.size;
                poolFireView.Add(entity).view = ps;

                //entity.EntityAddComponent<FireViewComponent2>(world);
                //poolGO.Add(entity).gameObject = ps.gameObject;
            }

            var filterBurned = world.Filter<BurnedOutComponent>().Inc<FireViewComponent>().End();
            foreach (var entity in filterBurned)
            {
                Object.Destroy(poolFireView.Get(entity).view.gameObject);
                poolFireView.Del(entity);

                entity.EntityWith<GameObjectComponent>(world, component => { Object.Destroy(component.GameObject); });
            }
        }
    }
}