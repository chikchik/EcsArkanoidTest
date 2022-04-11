using System.Collections.Generic;
using System.IO;
using Fabros.Ecs;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Fabros.Net.ClientServer.Ecs.Systems
{
    public class CreateGameSystem : IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            //world.SetEventsEnabled<>();

            var worldJson = File.ReadAllText("world.ecs.json");
            var dif = JsonUtility.FromJson<WorldDiff>(worldJson);

            var pool = world.GetUnique<LeoSharedComponent>().Pool;
            WorldUtils.ApplyDiff(pool, world, dif);
        }
    }
}