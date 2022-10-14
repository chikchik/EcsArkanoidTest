using System;
using Gaming.ContainerManager.ImageContracts.V1;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Server.Components;
using XFlow.Utils;

namespace XFlow.Server.Services
{
    public class UserService
    {
        private EcsWorld _mainWorld;
        private EcsWorld _inputWorld;
        private ComponentsCollection _components;
        private IInputService _inputService;
        
        
        private EcsPool<ClientComponent> _poolClients;
        private HGlobalReader _reader = new HGlobalReader();
        
        public UserService(IInputService inputService, EcsWorld inputWorld, 
            ComponentsCollection components)
        {
            _inputService = inputService;
            _inputWorld = inputWorld;
            _components = components;
            
        }

        public void SetMainWorld(EcsWorld mainWorld)
        {
            _mainWorld = mainWorld;
            _poolClients = _mainWorld.GetPool<ClientComponent>();
        }
        
        public void InputUserDisconnected(IUserAddress userAddress)
        {
            var inputEntity = _inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(_inputWorld);
            inputEntity.EntityAdd<UserDisconnectedInputComponent>(_inputWorld);
            inputEntity.EntityAdd<UserAddressComponent>(_inputWorld).Address = userAddress;
        }
        
        public void InputUserConnected(IUserAddress userAddress)
        {
            var inputEntity = _inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(_inputWorld);
            inputEntity.EntityAdd<UserConnectedInputComponent>(_inputWorld);
            inputEntity.EntityAdd<UserAddressComponent>(_inputWorld).Address = userAddress;
        }
        
        public void InputUserMessage(IUserAddress userAddress, ReadOnlyMemory<byte> data)
        {
            if (!PlayerService.TryGetPlayerEntityByPlayerId(_mainWorld, userAddress.UserId, out int playerEntity))
            {
                Debug.LogWarning($"not found player {userAddress.UserId}");
                return;
            }
            
            
            _reader.Init(data.ToArray());
            var inputTime = _reader.ReadInt32();
            var type = _reader.ReadInt32();
            
            var component = _components.GetComponent(type);
            
            ref var clientComponent = ref _poolClients.GetRef(playerEntity);
            clientComponent.UnreliableAddress = userAddress;
            
            var time = inputTime;


            var currentTick = _mainWorld.GetTick();
            var step = _mainWorld.GetUnique<TickDeltaComponent>().Value;
            //на сколько тиков мы опередили сервер или отстали
            var delay = time - currentTick;

            /*
             * delay > 0 - клиент опережает сервер
             * delay == 0 - клиент идет оптимально с сервером
             * delay < 0 клиент опоздал и тик на сервере уже прошел
             */

            //если ввод от клиента не успел прийти вовремя, то выполним его уже в текущем тике
            if (delay < 0)
                time = currentTick;

            var sentWorldTick = clientComponent.SentWorld.GetTick() - step.Value;

            if (delay == 0 && sentWorldTick == time)
                time = currentTick + step.Value;

            clientComponent.LastClientTick = inputTime;
            clientComponent.LastServerTick = currentTick;


            if (component.GetComponentType() == typeof(PingComponent)) //ping
            {
                clientComponent.LastPingTick = inputTime;
                clientComponent.Delay = delay;
                //var ms = _nextTickAt - DateTime.UtcNow;
                //clientComponent.DelayMs = ms.Milliseconds;
                //Debug.Log(clientComponent.DelayMs);
            }
            else
            {
                var componentData = component.ReadSingleComponent(_reader) as IInputComponent;
                _inputService.Input(_inputWorld, userAddress.UserId, time, componentData);
            }
            
            /*
            var inputEntity = _inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(_inputWorld);
            //inputEntity.EntityAdd<UserConnectedInputComponent>(inputWorld);
            inputEntity.EntityAdd<UserAddressComponent>(_inputWorld).Address = userAddress;*/
        }
    }
}