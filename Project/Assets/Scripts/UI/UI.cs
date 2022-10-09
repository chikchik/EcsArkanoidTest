using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;


using Game.ClientServer.Services;
using Game.State;
using XFlow.EcsLite;
using XFlow.Modules.States;
using XFlow.Modules.Tick.ClientServer.Components;

namespace Game.UI
{
    public class UI: 
        EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<AmmoCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<WeaponComponent>.IAnyComponentChangedListener,
        EventsSystem<TickComponent>.IAnyComponentChangedListener
    {
        public MainUI View { get; private set; }
        private EcsWorld _world;
        
        private PlayerControlService _controlService;
        private ClientServerServices _clientServerServices;
        private HpViewManager _hpViewManager;
        
        public UI(
            EcsWorld world, 
            MainUI view, 
            States states,
            PlayerControlService controlService,
            ClientServerServices clientServerServices,
            HpViewManager hpViewManager
            )
        {
            this.View = view;
            this._world = world;
            this._controlService = controlService;
            this._clientServerServices = clientServerServices;

            view.OnLateUpdate = () =>
            {
                _hpViewManager.LateUpdate();
            };

            _hpViewManager = hpViewManager;

            view.InteractionButton.onClick.AddListener(() =>
            {
                controlService.Interact();
            });
            
            view.KickButton.onClick.AddListener(() =>
            {
                controlService.Kick();
            });
            
            view.MechButton.onClick.AddListener(() =>
            {
                states.Push<MechInfoState>();
            });

            if (view.InventoryButton != null)
            {
                view.InventoryButton.onClick.AddListener(() =>
                {
                    states.Push<InventoryOpenedState>();
                });
            }

            view.ShotButton.onClick.AddListener(() =>
            {
                controlService.Shot();
            });

            view.FoodText.text = "";
            view.AmmoText.text = "0";
            view.ShotButton.gameObject.SetActive(false);

            var listener = world.CreateAnyListener();
            listener.SetAnyChangedListener<WeaponComponent>(this);
            listener.SetAnyChangedListener<FoodCollectedComponent>(this);
            listener.SetAnyChangedListener<AmmoCollectedComponent>(this);
            listener.SetAnyChangedListener<TickComponent>(this);
        }

        

        
        public void OnAnyComponentChanged(EcsWorld _, int entity, FoodCollectedComponent data, bool added)
        {
            if (!ClientBaseServices.IsControlledEntity(_world, entity))
                return;
            
            View.FoodText.text = $"Food Collected {data.Value}";
        }
        
        public void OnAnyComponentChanged(EcsWorld world, int entity, AmmoCollectedComponent data, bool added)
        {
            if (!ClientBaseServices.IsControlledEntity(_world, entity))
                return;

            View.AmmoText.text = data.Value.ToString();
        }

        public void OnAnyComponentChanged(EcsWorld _, int entity, WeaponComponent data, bool added)
        {
            if (!ClientBaseServices.IsControlledEntity(_world, entity))
                return;
            
            //var transform = entity.EntityGet<TransformComponent>(world).Transform
            View.ShotButton.gameObject.SetActive(true);
        }

        public void OnAnyComponentChanged(EcsWorld _, int tickEntity, TickComponent data, bool added)
        {
            //in case if anyone added not unique TickComponent
            //skip it
            if (tickEntity != 0)
                return;
            
            if (!ClientBaseServices.TryGetControlledEntity(_world, out int controlledEntity))
                return;

            var mechEntity = _clientServerServices.GetInteractionMechEntity(_world, controlledEntity);
            View.MechButton.gameObject.SetActive(mechEntity != -1);
        }
    }
}