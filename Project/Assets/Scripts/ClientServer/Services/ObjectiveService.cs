using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Game.Ecs.ClientServer.Components.Objective;
using Flow.EcsLite;

namespace Game.ClientServer
{
    public static class ObjectiveService
    {
        public static List<int> GetObjectivesByTarget(EcsWorld world, int targetEntity)
        {
            var entities = new List<int>();
            var objectives = world.Filter<ObjectiveTargetComponent>().End();
            var pool = world.GetPool<ObjectiveTargetComponent>();
            /*
             * найдем среди всех Objectives, те что завязаны на targetEntity
             */
            foreach (var entity in objectives)
            {
                if (pool.Get(entity).entity != targetEntity)
                    continue;
                entities.Add(entity);
            }

            return entities;
        }

        public static List<int> GetNextObjectives(EcsWorld world, int prevObjectiveEntity)
        {
            var entities = new List<int>();
            var objectives = world.Filter<ObjectivePrevComponent>().End();
            var pool = world.GetPool<ObjectivePrevComponent>();

            /*
             * находим задания которые открываются после prevObjectiveEntity
             */
            foreach (var entity in objectives)
            {
                if (pool.Get(entity).objectiveEntity != prevObjectiveEntity)
                    continue;
                entities.Add(entity);
            }

            return entities;
        }


        public static void Triggered(EcsWorld world, int entity)
        {
            var objectives = ObjectiveService.GetObjectivesByTarget(world, entity);
            objectives.ForEach(objEntity =>
            {
                if (!objEntity.EntityHas<ObjectiveCompletedComponent>(world))
                    objEntity.EntityAdd<ObjectiveCompletedComponent>(world);

                var nextObjectives = ObjectiveService.GetNextObjectives(world, objEntity);
                nextObjectives.ForEach(objEntity =>
                {
                    if (!objEntity.EntityHas<ObjectiveOpenedComponent>(world))
                        objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
                });
            });
        }
    }
}