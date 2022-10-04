using Game.ClientServer;
using Game.Ecs.Client.Components;
using Game.Ecs.View.Systems;

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
            NetClientPanel.Create(_client);
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