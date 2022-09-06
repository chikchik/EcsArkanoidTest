
using Game.Ecs.ClientServer.Components;
using Game.UIView;
using XFlow.EcsLite;
using XFlow.Modules.States;
using XFlow.Net.ClientServer;
using XFlow.Utils;

namespace Game.State
{
    public class MechInfoState : StateWithUI<MechInfoView>, 
        EventsSystem<ControlsMechComponent>.IComponentChangedListener,
        EventsSystem<ControlsMechComponent>.IComponentRemovedListener
    {
        private EcsWorld _world;
        private PlayerControlService _playerControlService;

        private int _unitEntity;
        
        public MechInfoState(
            States states,
            EcsWorld world, 
            MechInfoView view,
            PlayerControlService playerControlService):base(states)
        {
            this._view = view;
            this._world = world;
            this._playerControlService = playerControlService;
        }

        protected override void DoInitialize()
        {
            _view.ButtonClose.onClick.AddListener(() =>
            {
                Close();
            });
            
            _view.ButtonJoinMech.onClick.AddListener(() =>
            {
                _playerControlService.MechEnterLeave();
            });
            
            _view.ButtonLeaveMech.onClick.AddListener(() =>
            {
                _playerControlService.MechEnterLeave();
            });
        }

        protected override void DoEnter()
        {
            var playerId = _world.GetUnique<MainPlayerIdComponent>().value;
            _unitEntity = BaseServices.GetUnitEntityByPlayerId(_world, playerId);
            
            _unitEntity.AddChangedListener<ControlsMechComponent>(_world, this);
            _unitEntity.AddRemovedListener<ControlsMechComponent>(_world, this);

            UpdateButtonState();
        }

        protected override void DoExit()
        {
            _unitEntity.DelChangedListener<ControlsMechComponent>(_world, this);
            _unitEntity.DelRemovedListener<ControlsMechComponent>(_world, this);
        }


        private void UpdateButtonState()
        {
            var hasControl = _unitEntity.EntityHas<ControlsMechComponent>(_world);
            _view.ButtonJoinMech.gameObject.SetActive(!hasControl);
            _view.ButtonLeaveMech.gameObject.SetActive(hasControl);
        }

        public void OnComponentChanged(EcsWorld world, int entity, ControlsMechComponent data, bool newComponent)
        {
            UpdateButtonState();
            Close();
        }

        public void OnComponentRemoved(EcsWorld world, int entity, AlwaysNull<ControlsMechComponent> _)
        {
            UpdateButtonState();
            Close();
        }
    }
}