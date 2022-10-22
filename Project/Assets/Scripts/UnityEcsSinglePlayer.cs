using System.Collections.Generic;
using Game.Client.Services;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.UI;
using Game.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Net.Client;
using XFlow.Net.Client.Ecs.Components;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Services;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private EcsWorld _mainWorld;
        [Inject]
        private SingleGame _game;
        

        public void Start()
        {
            _game.PreInit();
            UnitySceneService.InitializeNewWorldFromScene(_mainWorld);
            ClientPlayerService.CreateSingleGamePlayer(_mainWorld);
            _game.Init();
        }
        
        public void Update()
        {
            _game.Update();
        }

        
        public void FixedUpdate()
        {
            if (!Application.isPlaying)
                return;
            _game.FixedUpdate();
        }
        

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
        
            EcsWorldDebugDraw.Draw(_mainWorld);
        }
    }
}