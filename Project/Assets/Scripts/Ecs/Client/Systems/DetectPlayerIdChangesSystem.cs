﻿using Fabros.Ecs.Utils;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.Client.Systems
{
    public class DetectPlayerIdChangesSystem : IEcsRunSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();

            if (!world.HasUnique<MainPlayerIdComponent>())
                return;

            var filter = world
                .Filter<LeoPlayerComponent>().IncChanges<LeoPlayerComponent>()
                .End();

            var mainPlayerID = world.GetUnique<MainPlayerIdComponent>().value;
            foreach (var entity in filter)
                if (entity.EntityGetComponent<LeoPlayerComponent>(world).id == mainPlayerID)
                {
                    //replace потому что ClientPlayerComponent мог уже быть 
                    world.ReplaceUnique<ClientPlayerComponent>().entity = entity;
                    entity.EntityReplaceComponent<LeoLerpComponent>(world).value = 1;
                }
        }
    }
}