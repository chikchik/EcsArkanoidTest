using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.ClientServer.Components;
using Fabros.EcsModules.Tick.Other;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Flow.EcsLite;

namespace Game.Ecs.ClientServer.Systems
{
    public class FootprintSystem : IEcsRunSystem
    {
        private const float FootprintLifeTime = 10f;
        private const float FootprintDelta = 0.5f;

        public void Run(EcsSystems systems)
        {
            var world = systems.GetWorld();
            var filter = world
                .Filter<UnitComponent>()
                .Inc<PositionComponent>()
                .Inc<LookDirectionComponent>()
                .End();
            var poolPosition = world.GetPool<PositionComponent>();
            //var poolMoveDirection = world.GetPool<MoveDirectionComponent>();
            var poolLookDirection = world.GetPool<LookDirectionComponent>();
            var poolLastFootprint = world.GetPool<LastFootprintComponent>();

            foreach (var entity in filter)
            {
                var currentPosition = poolPosition.Get(entity).value;

                if (!poolLastFootprint.Has(entity))
                {
                    poolLastFootprint.Add(entity).position = currentPosition;
                    continue;
                }

                if ((poolLastFootprint.Get(entity).position - currentPosition).magnitude > FootprintDelta)
                {
                    var direction = poolLookDirection.Get(entity).value;
                    ref var lastFootprint = ref poolLastFootprint.GetRef(entity);

                    lastFootprint.position = currentPosition;
                    lastFootprint.direction = direction;
                    lastFootprint.isLeftHand = !lastFootprint.isLeftHand;

                    CreateFootprintAtPosition(world, lastFootprint);
                }
            }
        }

        private void CreateFootprintAtPosition(EcsWorld world, LastFootprintComponent lastFootprint)
        {
            var footPrintEntity = world.NewEntity();
            ref var footprintComponent = ref footPrintEntity.EntityAddComponent<FootprintComponent>(world);
            footprintComponent.isLeftHand = lastFootprint.isLeftHand;
            footprintComponent.direction = lastFootprint.direction;

            ref var positionComponent = ref footPrintEntity.EntityAddComponent<PositionComponent>(world);
            positionComponent.value = lastFootprint.position;


            ref var lifeTimeComponent = ref footPrintEntity.EntityAddComponent<LifeTimeComponent>(world);
            //lifeTimeComponent.lifeTime = FootprintLifeTime;
            var ticks = (int) (150f / world.GetDeltaSeconds());
            lifeTimeComponent.destroyTick = world.GetUnique<TickComponent>().Value.Value + ticks;

            footPrintEntity.EntityAddComponent<FlammableComponent>(world).Power = 1;
        }
    }
}