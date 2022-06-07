using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Leopotam.EcsLite;
using Zenject;

namespace Game.Client
{
    public class UI: EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener
    {
        private MainUI view;
        private EcsWorld world;
        
        public UI(
            EcsWorld world, 
            [Inject(Id = "input")] EcsWorld inputWorld, 
            MainUI view)
        {
            this.view = view;
            this.world = world;

            view.InteractionButton.onClick.AddListener(() =>
            {
                var input = new UserInput
                {
                    hasInteraction = true,
                    action = new UserInput.Action()
                };

                InputService.ApplyInput(inputWorld, 1, input);
            });
            
            view.ShotButton.onClick.AddListener(() =>
            {
                var input = new UserInput
                {
                    hasInteraction = true,
                    action = new UserInput.Action()
                };

                InputService.ApplyInput(inputWorld, 1, input);
            });

            view.FoodText.text = "";

            var globalListenerEntity = world.NewEntity();
            globalListenerEntity.AddAnyChangedListener<FoodCollectedComponent>(world, this);


        }
        
        
        public void OnAnyComponentChanged(EcsWorld world, int entity, FoodCollectedComponent data, bool added)
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return;
            
            var unitEntity = world.GetUnique<ClientPlayerComponent>().entity;
            if (unitEntity != entity)
                return;

            view.FoodText.text = $"Food Collected {data.Value}";       
        }
    }
}