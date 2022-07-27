using Fabros.EcsModules.Mech.ClientServer;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Flow.EcsLite;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Box2D.ClientServer.Components.Colliders;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class CustomInitSystem : IEcsInitSystem
    {
        private MechService mechService;
        public CustomInitSystem(MechService mechService)
        {
            this.mechService = mechService;
        }
        
        public void Init(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var mechEntity = mechService.CreateMechEntity(world);
            mechEntity.EntityGetRef<PositionComponent>(world).value = new Vector3(5, 0, -15f);

            mechEntity.EntityAdd<InteractableComponent>(world);
            
            mechEntity.EntityAdd<AverageSpeedComponent>(world).Value = 8;


            ref var def = ref mechEntity.EntityAdd<Box2DRigidbodyDefinitionComponent>(world);
            def.Friction = 0.3f;
            def.Restitution = 0;
            def.RestitutionThreshold = 0.5f;   
            def.BodyType = BodyType.Kinematic;

            ref var collider = ref mechEntity.EntityAdd<Box2DCircleColliderComponent>(world);
            
            collider.Radius = 3;

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