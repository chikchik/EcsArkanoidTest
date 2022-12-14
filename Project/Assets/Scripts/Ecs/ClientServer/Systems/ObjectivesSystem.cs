using System.Collections.Generic;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;

using Game.ClientServer.Services;
using XFlow.EcsLite;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ObjectivesSystem : IEcsRunSystem, IEcsInitSystem
    {
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();

            var bushes = world.GetPool<BushComponent>().GetEntities();
            var boxes = world.GetPool<BoxComponent>().GetEntities();
            var buttons = world.GetPool<ButtonStateComponent>().GetEntities();
            var names = world.GetPool<GameObjectNameComponent>().GetEntities();

            int GetTargetEntityFrom(List<int> entities)
            {
                var entity = entities[0];
                entities.RemoveAt(0);
                return entity;
            }

            int GetTargetEntityByName(string name)
            {
                foreach (var entity in names)
                    if (entity.EntityGet<GameObjectNameComponent>(world).Name == name)
                    {
                        //те что часть квеста - не горят
                        if (entity.EntityHas<FlammableComponent>(world))
                            entity.EntityDel<FlammableComponent>(world);
                        return entity;
                    }


                return -1;
            }


            var chestWallEntity = GetTargetEntityByName("ChestWall");
            if (chestWallEntity == -1)
                return;
            
            int objEntity, objEntityA;
            objEntity = world.NewEntity();

            objEntity.EntityAdd<ObjectiveTargetEntityComponent>(world).Value = chestWallEntity;
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "Remove Red Wall";
            objEntityA = objEntity;


            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetEntityComponent>(world).Value = GetTargetEntityFrom(boxes);
            objEntity.EntityAdd<ObjectivePrevEntityComponent>(world).Value = objEntityA;
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "loot box";
            objEntityA = objEntity;

            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetEntityComponent>(world).Value = GetTargetEntityByName("BushObj");
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "eat berries";
            objEntityA = objEntity;
            
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetEntityComponent>(world).Value = GetTargetEntityByName("BushObj2");
            objEntity.EntityAdd<ObjectivePrevEntityComponent>(world).Value = objEntityA;
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "eat berries 2";
            objEntityA = objEntity;
            
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetEntityComponent>(world).Value = GetTargetEntityByName("BushObj3");
            objEntity.EntityAdd<ObjectivePrevEntityComponent>(world).Value = objEntityA;
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "eat berries 3";
            objEntityA = objEntity;


            /*
            objEntity = world.NewEntity();
            objEntity.EntityAdd<ObjectiveTargetComponent>(world).entity = GetTargetEntityFrom(buttons);
            objEntity.EntityAdd<ObjectiveOpenedComponent>(world);
            objEntity.EntityAdd<ObjectiveDescriptionComponent>(world).text = "push button 1";
            */
        }

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var openedGatesFilter = world.Filter<GateComponent>().IncChanges<GateOpenedComponent>()
                .End();

            foreach (var entity in openedGatesFilter)
            {
                ObjectiveService.Triggered(world, entity);
            }
        }
    }
}