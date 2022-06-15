using System.IO;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Serializer;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class CreateGameSystem : IEcsInitSystem
    {
        private readonly ComponentsCollection pool;

        public CreateGameSystem(ComponentsCollection pool)
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