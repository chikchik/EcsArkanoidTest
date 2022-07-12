using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;
using Leopotam.EcsLite;

namespace Game.UI
{
    public class UI: EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<WeaponComponent>.IAnyComponentChangedListener
    {
        public MainUI View { get; private set; }
        private EcsWorld world;
        private PlayerControlService controlService;
        
        public UI(
            EcsWorld world, MainUI view, PlayerControlService controlService)
        {
            this.View = view;
            this.world = world;
            this.controlService = controlService;

            view.InteractionButton.onClick.AddListener(() =>
            {
                controlService.Interact();
            });
            
            view.KickButton.onClick.AddListener(() =>
            {
                controlService.Kick();
            });
            
            view.ShotButton.onClick.AddListener(() =>
            {
                controlService.Shot();
            });

            view.FoodText.text = "";
            view.ShotButton.gameObject.SetActive(false);

            var listener = world.CreateAnyListener();
            listener.SetAnyChangedListener<WeaponComponent>(this);
            listener.SetAnyChangedListener<FoodCollectedComponent>(this);
        }

        
        public void OnAnyComponentChanged(EcsWorld world, int entity, FoodCollectedComponent data, bool added)
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return;
            
            var unitEntity = world.GetUnique<ClientPlayerComponent>().entity;
            if (unitEntity != entity)
                return;

            View.FoodText.text = $"Food Collected {data.Value}";       
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, WeaponComponent data, bool added)
        {
            //var transform = entity.EntityGet<TransformComponent>(world).Transform
            View.ShotButton.gameObject.SetActive(true);
        }
    }
}