using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;

using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class CustomInitSystem : IEcsInitSystem
    {
        private MechService _mechService;
        public CustomInitSystem(MechService mechService)
        {
            this._mechService = mechService;
        }
        
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var mechEntity = _mechService.CreateMechEntity(world);
            mechEntity.EntityGetRef<PositionComponent>(world).value = new Vector3(5, 0, -15f);

            mechEntity.EntityAdd<InteractableComponent>(world);
            
            mechEntity.EntityAdd<AverageSpeedComponent>(world).Value = 8;

            Box2DServices.AddRigidbodyDefinition(world, mechEntity, BodyType.Kinematic).SetFriction(0.3f).SetRestitutionThreshold(0.5f);
            Box2DServices.AddCircleColliderToDefinition(world, mechEntity, 3, Vector2.zero);

            /*
            var world = systems.GetWorld();
            var botEntity = UnitService.CreateUnitEntity(world);
            botEntity.EntityAddComponent<AIPlayerComponent>(world);

            botEntity.EntityAdd<FireComponent>(world) = new FireComponent
            {
                size = 1,
                endTime = 5,
                destroyEntity = false
            };*/
        }
    }
}