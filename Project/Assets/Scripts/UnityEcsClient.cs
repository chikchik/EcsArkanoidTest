using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.View.Systems;

using Game.Utils;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Net.Client;
using Zenject;

namespace Game
{
    public class UnityEcsClient : MonoBehaviour
    {
        [Inject] private Camera _camera;
        [Inject] private UI.UI _ui;
        [Inject] private Joystick _joystick;
        [Inject] private NetClient _client;
        
        [Inject] private PlayerControlService _controlService;
        
        [Inject] private EcsWorld _world;
        
        [Inject] 
        private EntityDestroyedListener _entityDestroyedListener;
        
        [Inject] 
        private ComponentsCollection _components;

        private EcsSystems _viewSystems;

        private void Start()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;

            
            _viewSystems = new EcsSystems(_world, "viewSystems");
            _viewSystems.Add(new SyncTransformSystem());
            _viewSystems.Add(new RotateCharacterSystem());

            _viewSystems.Add(new RotateRigidbodySystem());
            _viewSystems.Add(new CameraFollowSystem(Camera.main));
            
#if UNITY_EDITOR
            //viewSystems.Add(new XFlow.EcsLite.UnityEditor.EcsWorldDebugSystem(bakeComponentsInName:true));
#endif
            
            //глобальный обработчик удаления entity, чтоб никакой GameObject не утек
            //если случайно удалить entity самостоятельно или преждевременно
            //на сервере он тоже есть
            _world.EntityDestroyedListeners.Add(_entityDestroyedListener);
            
            
            _client.ConnectedAction = () =>
            {
                _viewSystems.Init();
            };
            
            

            string initialWorldJson = null;
            if (true)
            {
                var initialWorld = new EcsWorld("initial");
                ClientServices.InitializeNewWorldFromScene(initialWorld);
                var dif = WorldDiff.BuildDiff(_components, new EcsWorld("save"), initialWorld);
                initialWorldJson = dif.ToBase64String();
            }

            _client.Start(initialWorldJson);
        }

        private void Update()
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
            #endif
            
            if (!_client.Connected)
                return;

            _client.Update();
            
            UnityEcsSinglePlayer.CheckInput(_camera, _joystick, _controlService);
            
            _viewSystems.Run();

            _ui.View.DebugText.text = _client.GetDebugString();
        }

        private void OnDestroy()
        {
            if (_client == null)
                return;
            _client.OnDestroy();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            EcsWorldDebugDraw.Draw(_world);
        }
    }
}