using Fabros.Ecs.Utils;
using Flow.EcsLite;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer;
using Game.UI;
using Game.UIView;

namespace Game.State
{
    public class MechInfoState : StateWithUI<MechInfoView>, 
        EventsSystem<ControlsMechComponent>.IComponentChangedListener,
        EventsSystem<ControlsMechComponent>.IComponentRemovedListener
    {
        private EcsWorld world;
        private PlayerControlService playerControlService;

        private int unitEntity;
        
        public MechInfoState(
            States states,
            EcsWorld world, 
            MechInfoView view,
            PlayerControlService playerControlService):base(states)
        {
            this.view = view;
            this.world = world;
            this.playerControlService = playerControlService;
        }

        protected override void DoInitialize()
        {
            view.ButtonClose.onClick.AddListener(() =>
            {
                Close();
            });
            
            view.ButtonJoinMech.onClick.AddListener(() =>
            {
                playerControlService.MechEnterLeave();
            });
            
            view.ButtonLeaveMech.onClick.AddListener(() =>
            {
                playerControlService.MechEnterLeave();
            });
        }

        protected override void DoEnter()
        {
            var playerId = world.GetUnique<MainPlayerIdComponent>().value;
            unitEntity = BaseServices.GetUnitEntityByPlayerId(world, playerId);
            
            unitEntity.AddChangedListener<ControlsMechComponent>(world, this);
            unitEntity.AddRemovedListener<ControlsMechComponent>(world, this);

            UpdateButtonState();
        }

        protected override void DoExit()
        {
            unitEntity.DelChangedListener<ControlsMechComponent>(world, this);
            unitEntity.DelRemovedListener<ControlsMechComponent>(world, this);
        }


        private void UpdateButtonState()
        {
            var hasControl = unitEntity.EntityHas<ControlsMechComponent>(world);
            view.ButtonJoinMech.gameObject.SetActive(!hasControl);
            view.ButtonLeaveMech.gameObject.SetActive(hasControl);
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