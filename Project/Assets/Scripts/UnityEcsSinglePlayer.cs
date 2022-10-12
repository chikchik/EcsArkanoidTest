using System.Collections.Generic;
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
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class UnityEcsSinglePlayer: MonoBehaviour
    {
        [Inject] private EcsWorld _mainWorld;
        [Inject(Id = EcsWorlds.Input)] private EcsWorld _inputWorld;
       
        [Inject]
        private SingleGame _game;
        

        private int _unitEntity = -1;
        private int _playerEntity = -1;
        
        private int _playerId = 1;

        public void Start()
        {
            _game.PreInit();
            
            ClientServices.InitializeNewWorldFromScene(_mainWorld);
            _mainWorld.AddUnique<MainPlayerIdComponent>().Value = "1";
            var playerEntity = PlayerService.CreatePlayerEntity(_mainWorld, "1");
            PlayerService.InputJoinPlayer(_mainWorld, _inputWorld, "1", playerEntity);
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