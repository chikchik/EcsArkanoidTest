using Game.Ecs.ClientServer.Components;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Utils;
using XFlow.Utils;

namespace Game.ClientServer.Services
{
    public class GameEntityFactory
    {
        public Vector2 PlatformSize { get; } = new Vector2(3f, 1f);
        public Vector2 BrickSize { get; } = new Vector2(2f, 1f);
        public float BallRadius { get; } = 0.5f;

        public int CreatePlatform(EcsWorld world, int playerEntity)
        {
            var entity = world.NewEntity();

            entity.EntityAdd<UnitComponent>(world).PlayerEntity = world.PackEntity(playerEntity);
            entity.EntityAdd<PositionComponent>(world);
            entity.EntityAdd<Rotation2DComponent>(world);
            entity.EntityAdd<PickuperComponent>(world);

            Box2DServices.AddRigidbodyDefinition(world, entity, BodyType.Dynamic)
                .SetDensity(700f)
                .SetFriction(0f)
                .SetRestitution(0f)
                .SetRestitutionThreshold(0.5f)
                .SetLinearDamping(10f)
                .SetAngularDamping(10000)
                .SetGroupIndex(-1)
                .SetIsFreezeRotation(true)
                .SetSleepingAllowed(false);
            Box2DServices.AddBoxColliderToDefinition(world, entity, PlatformSize, Vector2.zero, 0f);

            entity.EntityAdd<AverageSpeedComponent>(world).Value = 15f;

            playerEntity.EntityAdd<ControlledEntityComponent>(world).Value = world.PackEntity(entity);
            playerEntity.EntityAdd<PrimaryUnitEntityComponent>(world).Value = world.PackEntity(entity);

            return entity;
        }

        public int CreateBall(EcsWorld world, Vector3 position, Vector2 force)
        {
            var entity = world.NewEntity();

            entity.EntityAdd<BallComponent>(world);
            entity.EntityAdd<PositionComponent>(world).Value = position;
            entity.EntityAdd<Rotation2DComponent>(world);
            entity.EntityAdd<BoardBoundsTrackableComponent>(world);
            entity.EntityAdd<AutoCleanableComponent>(world);

            var customHash = HashUtils.CustomHash(entity, world.GetTick());
            entity.EntityAdd<UniqueIdComponent>(world).Value = customHash;

            Box2DServices.AddRigidbodyDefinition(world, entity, BodyType.Dynamic)
                .SetDensity(1f)
                .SetFriction(1f)
                .SetRestitution(1f)
                .SetRestitutionThreshold(0.5f)
                .SetLinearDamping(0f)
                .SetAngularDamping(0f)
                .SetGroupIndex(-2)
                .SetSleepingAllowed(false);
            Box2DServices.AddCircleColliderToDefinition(world, entity, BallRadius, Vector2.zero);
            Box2DServices.CreateBodyNow(world, entity);

            var body = entity.EntityGet<Box2DBodyComponent>(world).BodyReference;
            Box2DApiSafe.ApplyForceToCenter(body, force);

            return entity;
        }

        public int CreateBrick(EcsWorld world, Vector3 position, float hp)
        {
            var entity = world.NewEntity();

            entity.EntityAdd<BrickComponent>(world);
            entity.EntityAdd<PositionComponent>(world).Value = position;
            entity.EntityAdd<Rotation2DComponent>(world);
            entity.EntityAdd<HpComponent>(world).Value = hp;
            entity.EntityAdd<MaxHpComponent>(world).Value = hp;
            entity.EntityAdd<AutoCleanableComponent>(world);

            Box2DServices.AddRigidbodyDefinition(world, entity, BodyType.Static)
                .SetDensity(700f)
                .SetFriction(0f)
                .SetRestitution(0f)
                .SetRestitutionThreshold(0.5f)
                .SetLinearDamping(10f)
                .SetAngularDamping(10000)
                .SetIsFreezeRotation(true)
                .SetSleepingAllowed(false);
            Box2DServices.AddBoxColliderToDefinition(world, entity, BrickSize, Vector2.zero, 0f);

            return entity;
        }

        public int CreateDropBonus(EcsWorld world, Vector3 position, BonusType bonusType)
        {
            var entity = world.NewEntity();

            entity.EntityAdd<BonusComponent>(world).BonusType = bonusType;
            entity.EntityAdd<PositionComponent>(world).Value = position;
            entity.EntityAdd<Rotation2DComponent>(world);
            entity.EntityAdd<BoardBoundsTrackableComponent>(world);
            entity.EntityAdd<PickupableComponent>(world);
            entity.EntityAdd<AverageSpeedComponent>(world).Value = 5f;
            entity.EntityAdd<TargetPositionComponent>(world).Value = position + Vector3.back * 20f;
            entity.EntityAdd<AutoCleanableComponent>(world);

            Box2DServices.AddRigidbodyDefinition(world, entity, BodyType.Dynamic)
                .SetDensity(1f)
                .SetFriction(1f)
                .SetRestitution(1f)
                .SetRestitutionThreshold(0.5f)
                .SetLinearDamping(0f)
                .SetAngularDamping(0f)
                .SetIsFreezeRotation(true)
                .SetIsTrigger(true)
                .SetGroupIndex(-3)
                .SetSleepingAllowed(false);
            Box2DServices.AddBoxColliderToDefinition(world, entity, PlatformSize, Vector2.zero, 0f);

            return entity;
        }
    }
}