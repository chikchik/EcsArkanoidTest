using System;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Components;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game.Client
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private PlayerInput.PlayerInput playerInput;
        [Inject] private UI ui;
        [Inject] private EcsWorld world;
        
        
        private EcsWorld inputWorld;
        private EcsSystems systems;
        private EcsSystems viewSystems;

        private int unitEntity = -1;
        private int playerId = 1;

        public void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;
            
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
                Global = global
            });

            
            unitEntity = UnitService.CreateUnitEntity(world);
            world.AddUnique(new ClientPlayerComponent{ entity = unitEntity});
            
            
#if UNITY_EDITOR
            systems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif

            var systemsFactory = new EcsSystemsFactory(null);
            
            systemsFactory.AddNewSystems(systems, 
                new IEcsSystemsFactory.Settings{client = true, server = true});

            systems.Init();
            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem(true));
            viewSystems.Add(new RotateCharacterSystem());

            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem(Camera.main));
            
            viewSystems.Init();
            
            world.AddUnique(new MainPlayerIdComponent{value = playerId});
            unitEntity.EntityAdd<PlayerComponent>(world).id = playerId;
            
        }
        
        public void Update()
        {
            UnityEcsClient.CheckInput(world, unitEntity, playerInput, camera, input =>
            {
                if (world.HasUnique<RootMotionComponent>())
                {
                    //todo, dublicated code
                    input.hasUnitPos = true;
                    input.unitPos = world.GetUnique<RootMotionComponent>().Position;
                }

                InputService.ApplyInput(inputWorld, playerId, input);
            });

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var input = new UserInput{player = playerId, hasInteraction = true};
                InputService.ApplyInput(inputWorld, playerId, input);
            }
            viewSystems.Run();
        }

        public void FixedUpdate()
        {
            if (!Application.isPlaying)
                return;
            systems.Run();
        }
        

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
        
            DebugDraw.Draw(world);
        }
    }
}