using System;
using XFlow.EcsLite;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct BulletHitComponent
    {
        public BulletComponent Bullet;
        public EcsPackedEntity EntityHit;
    }
}