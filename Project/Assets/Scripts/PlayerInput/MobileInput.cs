using Game.UI;
using UnityEngine;

namespace Game.PlayerInput
{
    public class MobileInput : PlayerInput
    {
        private readonly Joystick joystick;
        private readonly MainUI mainUI;
        private bool hasInteraction;

        public MobileInput(Joystick joystick, MainUI mainUI)
        {
            this.joystick = joystick;
            this.mainUI = mainUI;
        }

        public override bool HasTouch => Input.touchCount > 0 && Movement.magnitude <= 0;
        public override Vector3 TouchPosition => Input.GetTouch(0).position;
        protected override float Horizontal => joystick.Horizontal;
        protected override float Vertical => joystick.Vertical;

        public override void Initialize()
        {
            mainUI.InteractionButton.onClick.AddListener(OnInteractionButtonClick);
        }

        public override void Dispose()
        {
            mainUI.InteractionButton.onClick.RemoveListener(OnInteractionButtonClick);
        }

        public override void Tick()
        {
            hasInteraction = false;
        }

        private void OnInteractionButtonClick()
        {
            Debug.Log("OnInteractionButtonClick");
            hasInteraction = true;
        }
    }
}