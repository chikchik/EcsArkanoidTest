using System.Collections.Generic;
using Fabros.EcsModules.Grid.Other;
using Fabros.EcsModules.Mech.ClientServer.Components;
using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    
    public class ClientServerServices//todo find better name
    {
        List<int> entities = new List<int>();
        
        public int GetInteractionMechEntity(EcsWorld world, int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                if (unitEntity.EntityGet<ControlsMechComponent>(world).PackedEntity.Unpack(world, out int entity))
                    return entity;
                
                return -1;
            }
            
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).value,
                1, ref entities, entity=> entity.EntityHas<MechComponent>(world));

            if (entities.Count == 0)
                return -1;
            
            return entities[0];
        }
    }
}