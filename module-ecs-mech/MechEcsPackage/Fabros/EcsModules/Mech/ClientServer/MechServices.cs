
using Flow.EcsLite;

namespace Fabros.EcsModules.Mech.ClientServer
{
    public class MechServices
    {
        public int CreateUnitEntity(EcsWorld world)
        {
            var entity = world.NewEntity();
            
            return entity;
        }
    }
}