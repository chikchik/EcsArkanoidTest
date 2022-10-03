using System.Collections.Generic;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.View.Systems;

using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
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
       
        [Inject]
        private SingleGame _game;
        

        private int _unitEntity = -1;
        private int _playerId = 1;

        public void Start()
        {
            _game.PreInit();
            
            ClientServices.InitializeNewWorldFromScene(_mainWorld);
            
            _unitEntity = UnitService.CreateUnitEntity(_mainWorld);
            _mainWorld.AddUnique(new ClientPlayerComponent{ entity = _unitEntity});
            
            _mainWorld.AddUnique(new MainPlayerIdComponent{value = _playerId});
            _unitEntity.EntityAdd<PlayerComponent>(_mainWorld).id = _playerId;

            var inventory = _mainWorld.NewEntity();
            inventory.EntityAdd<InventoryComponent>(_mainWorld).SlotCapacity = 10;
            
            var trash = _mainWorld.NewEntity();
            trash.EntityAdd<InventoryComponent>(_mainWorld).SlotCapacity = 10;

            _unitEntity.EntityAdd<InventoryLinkComponent>(_mainWorld).Inventory = _mainWorld.PackEntity(inventory);
            _unitEntity.EntityAdd<TrashLinkComponent>(_mainWorld).Trash = _mainWorld.PackEntity(trash);
            
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