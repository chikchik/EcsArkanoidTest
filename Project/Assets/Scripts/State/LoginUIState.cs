using Game.Client.Services;
using Game.Ecs.ClientServer.Components;
using Game.UI;
using Game.UIView;
using XFlow.EcsLite;
using XFlow.Modules.States;
using XFlow.Utils;

namespace Game.State
{
    public class LoginUIState : StateWithUI<LoginUIView>,
        EventsSystem<NicknameComponent>.IComponentChangedListener
    {
        private readonly EcsWorld _world;
        private readonly PlayerControlService _playerControlService;

        public LoginUIState(
            States states,
            EcsWorld world,
            LoginUIView view,
            PlayerControlService playerControlService) : base(states)
        {
            _world = world;
            _view = view;
            _playerControlService = playerControlService;
        }

        protected override void DoInitialize()
        {
            _view.ButtonLogin.onClick.AddListener(() =>
            {
                Login(_view.NicknameInputField.text);
            });
        }

        protected override void DoEnter()
        {
            if (!ClientPlayerService.TryGetPlayerEntity(_world, out int playerEntity))
                return;

            playerEntity.AddChangedListener<NicknameComponent>(_world, this);
        }

        protected override void DoExit()
        {
            if (!ClientPlayerService.TryGetPlayerEntity(_world, out int playerEntity))
                return;

            playerEntity.DelChangedListener<NicknameComponent>(_world, this);
        }

        public void OnComponentChanged(EcsWorld world, int entity, NicknameComponent data, bool newComponent)
        {
            Close();
        }

        private void Login(string nickname)
        {
            if (!string.IsNullOrWhiteSpace(nickname))
            {
                _playerControlService.Login(nickname);
            }
        }
    }
}