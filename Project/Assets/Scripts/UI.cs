using System;
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
        private PlayerControlService controlService;
        
        public UI(
            EcsWorld world, MainUI view, PlayerControlService controlService)
        {
            this.view = view;
            this.world = world;
            this.controlService = controlService;

            view.InteractionButton.onClick.AddListener(() =>
            {
                
            });
            
            view.ShotButton.onClick.AddListener(() =>
            {
                controlService.Shot();
            });

            view.FoodText.text = "";

            var globalListenerEntity = world.NewLocalEntity();
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