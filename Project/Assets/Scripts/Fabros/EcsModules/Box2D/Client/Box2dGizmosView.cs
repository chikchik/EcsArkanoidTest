using System;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using UnityEngine;

namespace Fabros.EcsModules.Box2D.Client
{
    public class Box2dGizmosView : MonoBehaviour
    {
        private IntPtr box2d;
        
        [SerializeField]
        private bool draw = true;

        public void SetBox2D(IntPtr box2d)
        {
            this.box2d = box2d;
        }
        
        private void OnDrawGizmos()
        {
            if (box2d == default)
                return;
            if (!draw)
                return;

            Box2DApi.DebugDraw(box2d);
        }
    }
}