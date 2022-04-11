using System.Collections.Generic;
using Game.Client;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.Client.Systems
{
    public class InitSceneSystem : IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            /*при самом первом запуске клиента в юнити сцене игры есть много разных объектов
             которые уже могли быть уничтожены на сервере
             потому надо пройтись по серверным GameObjects и определить лишние на клиенте                 
             */

            var world = systems.GetWorld();

            var sceneWorld = new EcsWorld("fromScene");
            ClientServices.InitializeNewWorldFromScene(sceneWorld);

            var poolSceneWorldObjects = sceneWorld.GetPool<GameObjectComponent>();
            var poolSceneWorldObjectNames = sceneWorld.GetPool<GameObjectNameComponent>();

            var poolWorldObjectNames = world.GetPool<GameObjectNameComponent>();

            var sceneEntitiesByName = new Dictionary<string, GameObject>();

            foreach (var entity in sceneWorld.Filter<GameObjectNameComponent>().End())
            {
                var name = poolSceneWorldObjectNames.Get(entity).Name;
                sceneEntitiesByName[name] = poolSceneWorldObjects.Get(entity).GameObject;
            }

            foreach (var entity in world.Filter<GameObjectNameComponent>().End())
                sceneEntitiesByName.Remove(poolWorldObjectNames.Get(entity).Name);

            /*
             * все кто остался в sceneEntitiesByName можно удалить со сцены
             */

            sceneEntitiesByName.Values.ForEach(go => { Object.Destroy(go); });


            ClientServices.LinkUnits(world);
        }
    }
}