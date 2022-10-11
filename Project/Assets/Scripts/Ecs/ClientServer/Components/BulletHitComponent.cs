using System;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct BulletHitComponent
    {
        public BulletDamageComponent Bullet;
        public EcsPackedEntity EntityHit;
    }
}