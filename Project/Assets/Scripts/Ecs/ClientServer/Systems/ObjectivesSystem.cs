﻿using System.Collections.Generic;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Leopotam.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class ObjectivesSystem : IEcsRunSystem, IEcsInitSystem
    {
        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var openedGatesFilter = world.Filter<GateComponent>().IncChanges<GateOpenedComponent>()
                .End();
            
            foreach (var entity in openedGatesFilter)
            {
                var objectives = ObjectiveService.GetObjectivesByTarget(world, entity);
                objectives.ForEach(objEntity =>
                {
                    if (!objEntity.EntityHas<ObjectiveCompletedComponent>(world))
                    {
                        objEntity.EntityAdd<ObjectiveCompletedComponent>(world);
                    }

                    var nextObjectives = ObjectiveService.GetNextObjectives(world, objEntity);
                    nextObjectives.ForEach(objEntity =>
                    {
                        if (!objEntity.EntityHas<ObjectiveOpenedComponent>(world))
                        {
                            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
                        }
                    });
                });
            }
        }

        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            
            var bushes = world.GetPool<BushComponent>().GetEntities();
            var boxes = world.GetPool<BoxComponent>().GetEntities();
            var buttons = world.GetPool<ButtonComponent>().GetEntities();
            var names = world.GetPool<GameObjectNameComponent>().GetEntities();

            int GetTargetEntityFrom(List<int> entities)
            {
                int entity = entities[0];
                entities.RemoveAt(0);
                return entity;
            }
            
            int GetTargetEntityByName(string name)
            {
                foreach (var entity in names)
                {
                    if (entity.EntityGet<GameObjectNameComponent>(world).Name == name)
                    {
                        //те что часть квеста - не горят
                        if (entity.EntityHas<FlammableComponent>(world))
                            entity.EntityDel<FlammableComponent>(world);
                        return entity;
                    }
                }

                
                return -1;
            }
            
            
            int objEntity, objEntityA;
            
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetComponent>(world).entity = GetTargetEntityByName("ChestWall");
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "Remove Red Wall";
            objEntityA = objEntity;
            
            
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetComponent>(world).entity = GetTargetEntityFrom(boxes);
            objEntity.EntityAdd<ObjectivePrevComponent>(world).objectiveEntity = objEntityA;
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "loot box";
            objEntityA = objEntity;
            
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetComponent>(world).entity = GetTargetEntityByName("BushObj");
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "eat berries";
            objEntityA = objEntity;
            
            
            /*
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetComponent>(world).entity = GetTargetEntityFrom(buttons);
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "push button 1";
            */
        }
    }
}