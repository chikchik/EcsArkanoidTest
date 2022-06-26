using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Leopotam.EcsLite;

#if CLIENT
using UnityEngine;
using Fabros.Ecs.Client.Components;
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
            if (entity.EntityHasComponent<TransformComponent>(world))
            {
                var go = entity.EntityGetComponent<TransformComponent>(world).Transform.gameObject;
                GameObject.Destroy(go);
            }
#endif
            
            if (entity.EntityHas<Box2DBodyComponent>(world))
            {
               // Box2DApi.DestroyBody2(entity.EntityGet<Box2DBodyComponent>(world).BodyReference);
            }
        }
    }
}