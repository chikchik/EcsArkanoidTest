using Game.Ecs.ClientServer.Components;

using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class FootprintSystem : IEcsRunSystem
    {
        private const float FootprintLifeTime = 10f;
        private const float FootprintDelta = 0.5f;

        public void Run(EcsSystems systems)
        {
            return;
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
            ref var footprintComponent = ref footPrintEntity.EntityAdd<FootprintComponent>(world);
            footprintComponent.isLeftHand = lastFootprint.isLeftHand;
            footprintComponent.direction = lastFootprint.direction;

            ref var positionComponent = ref footPrintEntity.EntityAdd<PositionComponent>(world);
            positionComponent.value = lastFootprint.position;


            ref var lifeTimeComponent = ref footPrintEntity.EntityAdd<LifeTimeComponent>(world);
            //lifeTimeComponent.lifeTime = FootprintLifeTime;
            var ticks = (int) (150f / world.GetDeltaSeconds());
            lifeTimeComponent.destroyTick = world.GetUnique<TickComponent>().Value.Value + ticks;

            footPrintEntity.EntityAdd<FlammableComponent>(world).Power = 1;
        }
    }
}