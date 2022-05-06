using System;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Client
{
    public class UnityEcsSinglePlayer: MonoBehaviour, EventsSystem<FoodCollectedComponent>.IAnyComponentChangedListener
    {
        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private PlayerInput.PlayerInput playerInput;
        [Inject] private MainUI ui;
        [Inject] private EcsWorld world;
        
        
        private EcsWorld inputWorld;
        private EcsSystems systems;
        private EcsSystems viewSystems;

        private int unitEntity;

        public void Start()
        {
            inputWorld = new EcsWorld("input");
            
            systems = new EcsSystems(world);
            systems.AddWorld(inputWorld, "input");
            ClientServices.InitializeNewWorldFromScene(world);
            
            world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta(1, (int)(1f/Time.fixedDeltaTime))
            });
            
            world.AddUnique(new TickComponent{Value = new Tick(0)});
            world.AddUnique(new ClientViewComponent
            {
                Camera = camera,
                Global = global,
                MainUI = ui
            });

            unitEntity = UnitService.CreateUnitEntity(world);
            unitEntity.EntityAdd<PlayerComponent>(world).id = 0;
            
            world.AddUnique(new ClientPlayerComponent{ entity = unitEntity});


            SystemsAndComponents.AddSystems(null, systems, true, true);
            systems.Init();
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem());
            viewSystems.Add(new RotateCharacterSystem());
            viewSystems.Add(new CameraFollowSystem());
            
            viewSystems.Init();
            
            
            ui.InteractionButton.onClick.AddListener(() =>
            {
                var input = new UserInput
                {
                    hasInteraction = true,
                    action = new UserInput.Action()
                };

                InputService.ApplyInput(inputWorld, 0, input);
            });

            ui.FoodText.text = "";

            int globalListenerEntity = world.NewEntity();
            globalListenerEntity.AddAnyChangedListener<FoodCollectedComponent>(world, this);
        }

        public void Update()
        {
            UnityEcsClient.CheckInput(world, unitEntity, playerInput, camera, input =>
            {
                InputService.ApplyInput(inputWorld, 0, input);
            });
            viewSystems.Run();
        }

        public void FixedUpdate()
        {
            systems.Run();
        }
        
        public void OnAnyComponentChanged(EcsWorld world, int entity, FoodCollectedComponent data, bool added)
        {
            if (!world.HasUnique<ClientPlayerComponent>())
                return;
            
            var unitEntity = world.GetUnique<ClientPlayerComponent>().entity;
            if (unitEntity != entity)
                return;

            ui.FoodText.text = $"Food Collected {data.Value}";       
        }
    }
}