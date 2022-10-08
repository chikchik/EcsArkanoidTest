using System.Collections.Generic;
using Fabros.EcsModules.Mech.ClientServer.Components;

using Game.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Grid.Other;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    
    public class ClientServerServices//todo find better name
    {
        List<int> _entities = new List<int>();
        
        public int GetInteractionMechEntity(EcsWorld world, int unitEntity)
        {
            if (unitEntity.EntityHas<ControlsMechComponent>(world))
            {
                if (unitEntity.EntityGet<ControlsMechComponent>(world).PackedEntity.Unpack(world, out int entity))
                    return entity;
                
                return -1;
            }
            
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).Value,
                1, ref _entities, entity=> entity.EntityHas<MechComponent>(world));

            if (_entities.Count == 0)
                return -1;
            
            return _entities[0];
        }
    }
}