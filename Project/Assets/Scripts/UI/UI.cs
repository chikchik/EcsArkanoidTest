﻿using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;


using Game.ClientServer.Services;
using Game.State;
using XFlow.EcsLite;
using XFlow.Modules.States;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Net.ClientServer;

namespace Game.UI
{
    public class UI: 
        EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<AmmoCollectedComponent>.IAnyComponentChangedListener,
        EventsSystem<WeaponComponent>.IAnyComponentChangedListener,
        EventsSystem<TickComponent>.IAnyComponentChangedListener
    {
        public MainUI View { get; private set; }
        private EcsWorld world;
        
        private PlayerControlService controlService;
        private ClientServerServices clientServerServices;
        
        public UI(
            EcsWorld world, 
            MainUI view, 
            States states,
            PlayerControlService controlService,
            ClientServerServices clientServerServices)
        {
            this.View = view;
            this.world = world;
            this.controlService = controlService;
            this.clientServerServices = clientServerServices;


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

        private bool IsPlayerUnitEntity(int entity)
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return false;
            
            return world.GetUnique<ClientPlayerComponent>().entity == entity;
        }
        
        private int GetPlayerUnitEntity()
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return -1;
            
            return world.GetUnique<ClientPlayerComponent>().entity;
        }

        
        public void OnAnyComponentChanged(EcsWorld _, int entity, FoodCollectedComponent data, bool added)
        {
            if (!IsPlayerUnitEntity(entity))
                return;
            
            View.FoodText.text = $"Food Collected {data.Value}";
        }
        
        public void OnAnyComponentChanged(EcsWorld world, int entity, AmmoCollectedComponent data, bool added)
        {
            if (!IsPlayerUnitEntity(entity))
                return;

            View.AmmoText.text = data.Value.ToString();
        }

        public void OnAnyComponentChanged(EcsWorld _, int entity, WeaponComponent data, bool added)
        {
            if (!IsPlayerUnitEntity(entity))
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

            var unitEntity = GetPlayerUnitEntity();
            if (unitEntity == -1)
                return;

            var mechEntity = clientServerServices.GetInteractionMechEntity(world, unitEntity);
            View.MechButton.gameObject.SetActive(mechEntity != -1);
        }
    }
}