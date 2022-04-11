using UnityEngine;

namespace Game.PlayerInput
{
    public class MobileInput : PlayerInput
    {
        private readonly Joystick  joystick;
        private readonly UI.MainUI mainUI;
        private          bool      hasInteraction;

        public MobileInput(Joystick joystick, UI.MainUI mainUI)
        {
            this.joystick = joystick;
            this.mainUI = mainUI;
        }

        public override bool HasInteraction => hasInteraction;
        public override bool HasTouch => Input.touchCount > 0 && Movement.magnitude <= 0 && !HasInteraction;
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