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
        
        public bool TryGetInteractableMechEntity(EcsWorld world, int unitEntity, out int mechEntity)
        {
            mechEntity = -1;
            if (unitEntity.EntityHas<MechComponent>(world))
                return false;
            
            world.GetNearestEntities(unitEntity,
                unitEntity.EntityGet<PositionComponent>(world).Value,
                1, ref _entities, entity=> entity.EntityHas<MechComponent>(world));

            if (_entities.Count == 0)
                return false;
            mechEntity = _entities[0];
            return true;
        }
    }
}