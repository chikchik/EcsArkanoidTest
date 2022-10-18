using Cysharp.Threading.Tasks;
using Game.Client.Services;
using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Utils;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Net.Client;
using XFlow.Net.Client.DebugUtils;
using Zenject;

namespace Game
{
    public class UnityEcsClient : MonoBehaviour
    {
        [Inject] private UI.UI _ui;
        [Inject] private EcsWorld _mainWorld;
        
        [Inject] 
        private ComponentsCollection _components;
        
        [Inject] private NetClient _client;
        
        [Inject] private DiContainer _container;

        private void Start()
        {
            NetClientPanel.Create(_client).Show();
            ConnectAsync().Forget();
        }

        private async UniTaskVoid ConnectAsync()
        {
            byte[] initialWorldJson = null;
            if (true)
            {
                var initialWorld = new EcsWorld("initial");
                UnitySceneService.InitializeNewWorldFromScene(initialWorld);
                var dif = WorldDiff.BuildDiff(_components, new EcsWorld("save"), initialWorld);
                initialWorldJson = dif.ToByteArray(true);
            }

            await _client.Connect(initialWorldJson);
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
            EcsWorldDebugDraw.Draw(_mainWorld);
        }
    }
}