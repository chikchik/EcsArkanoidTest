using System;
using Fabros.Ecs;
using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Box2D.ClientServer.Api;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.Client.Systems;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.View.Systems;
using Game.Fabros.EcsModules.Fire.Client.Components;
using Game.Fabros.Net.Client;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Protocol;
using Game.UI;
using Game.Utils;
using Leopotam.EcsLite;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Game.Client
{
    public class UnityEcsClient : MonoBehaviour
    {
        [Inject] private Camera camera;
        [Inject] private Global global;
        [Inject] private UI ui;
        [Inject] private Joystick joystick;
        [Inject] private NetClient client;
        
        [Inject] private PlayerControlService controlService;
        
        [Inject] private EcsWorld world;
        
        [Inject] 
        private EntityDestroyedListener entityDestroyedListener;
        
        [Inject] 
        private ComponentsCollection components;

        private EcsSystems viewSystems;

        private void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            
            viewSystems = new EcsSystems(world);
            viewSystems.Add(new SyncTransformSystem(false));
            viewSystems.Add(new RotateCharacterSystem());

            viewSystems.Add(new RotateRigidbodySystem());
            viewSystems.Add(new CameraFollowSystem(Camera.main));
            
#if UNITY_EDITOR
            viewSystems.Add(new Leopotam.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif
            
            //глобальный обработчик удаления entity, чтоб никакой GameObject не утек
            //если случайно удалить entity самостоятельно или преждевременно
            //на сервере он тоже есть
            world.EntityDestroyedListeners.Add(entityDestroyedListener);
            
            
            client.ConnectedAction = () =>
            {
                viewSystems.Init();
            };
            
            client.InitWorldAction = world =>
            {
                var viewComponent = new ClientViewComponent();
                viewComponent.Global = global;

                world.AddUnique<ClientViewComponent>() = viewComponent;
            };
            

            string initialWorldJson = null;
            if (true)
            {
                var initialWorld = new EcsWorld("initial");
                ClientServices.InitializeNewWorldFromScene(initialWorld);
                var dif = WorldDiff.BuildDiff(components, new EcsWorld("save"), initialWorld);
                initialWorldJson = dif.ToBase64String();
            }

            client.Start(initialWorldJson);
        }

        private void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            if (!client.Connected)
                return;

            client.Update();
            
            UnityEcsSinglePlayer.CheckInput(camera, joystick, controlService);
            
            viewSystems.Run();

            //ui.View.DebugText.text = client.GetDebugString();
        }

        private void OnDestroy()
        {
            if (client == null)
                return;
            client.OnDestroy();
        }

        private void OnGUI()
        {
            //client.OnGUI();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            EcsWorldDebugDraw.Draw(world);
        }
    }
}