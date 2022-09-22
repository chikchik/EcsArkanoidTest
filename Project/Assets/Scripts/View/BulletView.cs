using System;
using UnityEngine;
using XFlow.EcsLite;
using Zenject;

namespace Game.View
{
    public class BulletView : MonoBehaviour
    {
        [Inject] 
        private EcsWorld MainWorld;
        
        public EcsPackedEntity PackedEntity;

        public void Update()
        {
            if (MainWorld == null)
                return;
            
            if (PackedEntity.Unpack(MainWorld, out int entity))
            {
                Debug.Log($"bullet {entity} {PackedEntity.Id}");
            }
        }
    }
}