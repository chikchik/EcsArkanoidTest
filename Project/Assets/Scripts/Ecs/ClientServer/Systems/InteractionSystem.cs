using System;
using System.Collections.Generic;
using System.Linq;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid.Components;
using Game.Ecs.ClientServer.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Ecs.ClientServer.Systems
{
    public class InteractionSystem : IEcsRunSystem
    {
        private const int InteractionRadius = 3;

        private readonly Dictionary<int, bool> isAnyCharacterInteractWith = new();

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var poolInteractable = world.GetPool<InteractableComponent>();
            var poolPosition = world.GetPool<PositionComponent>();
            var poolRadius = world.GetPool<RadiusComponent>();

            var charactersFilter = world.Filter<UnitComponent>().End();
            isAnyCharacterInteractWith.Clear();

            foreach (var characterEntity in charactersFilter)
            {
                var characterPosition = poolPosition.Get(characterEntity).value;
                var gridCharacterPosition = new Vector2Int
                {
                    x = Convert.ToInt32(characterPosition.x),
                    y = Convert.ToInt32(characterPosition.z)
                };

                var entities = GetNearestEntitiesWithComponent<InteractableComponent>(world,
                    gridCharacterPosition, InteractionRadius);

                for (var i = 0; i < entities.Count; i++)
                {
                    var entity = entities[i];

                    ref var interactableComponent = ref poolInteractable.GetRef(entity);
                    var interactablePosition = poolPosition.Get(entity).value;
                    var radius = poolRadius.GetRef(entity).radius;
                    var distance = (characterPosition - interactablePosition).magnitude;

                    var isAnyCharacterInteract = isAnyCharacterInteractWith.ContainsKey(entity) &&
                                                 isAnyCharacterInteractWith[entity];

                    interactableComponent.canInteract = interactableComponent.isInteractable && distance <= radius ||
                                                        isAnyCharacterInteract;
                    isAnyCharacterInteractWith[entity] = interactableComponent.canInteract;
                }
            }
        }


        //todo rewrite with latest grid api

        public static List<int> GetNearestEntitiesWithComponent<T>(
            EcsWorld world, Vector2Int position, int radius)
            where T : struct
        {
            var gridComponent = world.GetUnique<GridComponent>();
            var gridMap = gridComponent.GridMap;
            var resultEntities = new List<int>();

            for (var x = position.x - radius; x <= position.x + radius; x++)
            for (var y = position.y - radius; y < position.y + radius; y++)
            {
                var gridPosition = new Vector2Int(x, y);

                if (!gridMap.ContainsKey(gridPosition)) continue;

                resultEntities.AddRange(gridMap[gridPosition]);
            }

            return resultEntities.Where(entity => entity.EntityHasComponent<T>(world)).ToList();
        }
    }
}