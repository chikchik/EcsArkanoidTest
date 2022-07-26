using System;
using System.Collections.Generic;

namespace Game.Ecs.ClientServer.Components
{
    [Serializable]
    public struct BulletHit
    {
        public Queue<BulletComponent> BulletHits;
    }
}