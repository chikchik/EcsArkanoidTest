using System.Collections.Generic;
using System.IO;
using Fabros.Ecs;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class CreateGameSystem : IEcsInitSystem
    {
        private ComponentsPool pool;
        public CreateGameSystem(ComponentsPool pool)
        {
            this.pool = pool;
        }
        
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var worldJson = File.ReadAllText("world.ecs.json");
            var dif = JsonUtility.FromJson<WorldDiff>(worldJson);

            WorldUtils.ApplyDiff(pool, world, dif);
        }
    }
}