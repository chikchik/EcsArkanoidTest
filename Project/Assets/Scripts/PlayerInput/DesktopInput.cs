using UnityEngine;

namespace Game.PlayerInput
{
    public class DesktopInput : PlayerInput
    {
        public override bool HasTouch => Input.GetMouseButtonDown(0);
        public override Vector3 TouchPosition => Input.mousePosition;
        protected override float Horizontal => Input.GetAxis("Horizontal");
        protected override float Vertical => Input.GetAxis("Vertical");
    }
}