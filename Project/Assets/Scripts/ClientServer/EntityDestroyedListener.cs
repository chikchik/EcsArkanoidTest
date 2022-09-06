using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Api;
using XFlow.Modules.Box2D.ClientServer.Components;
using XFlow.Utils;

#if CLIENT
using UnityEngine;
using XFlow.Ecs.Client.Components;
#endif

namespace Game.ClientServer
{
    /*
     * можно было бы разделить на 2 файла раздельных Client, ClientServer версии
     */
    public class EntityDestroyedListener : IEcsEntityDestroyedListener
    {
        public void OnEntityWillBeDestroyed(EcsWorld world, int entity)
        {
#if CLIENT
            if (entity.EntityHas<TransformComponent>(world))
            {
                var go = entity.EntityGet<TransformComponent>(world).Transform.gameObject;
                GameObject.Destroy(go);
            }
#endif
            
            if (entity.EntityHas<Box2DBodyComponent>(world))
            {
                Box2DApiSafe.DestroyBody(entity.EntityGet<Box2DBodyComponent>(world).BodyReference);
                entity.EntityDel<Box2DBodyComponent>(world);
            }
        }
    }
}