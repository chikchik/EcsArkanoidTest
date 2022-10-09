using System.Collections.Generic;
using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components.Input;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using XFlow.Modules.Tick.Other;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components.Input;
using XFlow.Utils;

namespace Game.Ecs.ClientServer.Systems
{
    public class ApplyDragInputSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsFilter _filter;
        private EcsWorld _world;
        private EcsWorld _inputWorld;

        private MyInventoryService _inventoryService;
        
        private List<int> _entities = new List<int>();

        public ApplyDragInputSystem(MyInventoryService myInventoryService)
        {
            _inventoryService = myInventoryService;
        }
        
        public void Init(EcsSystems systems)
        {
            _world = systems.GetWorld();
            _inputWorld = systems.GetWorld(EcsWorlds.Input);
            _filter = _inputWorld.Filter<InputComponent>().End();
        }
        
        public void Run(EcsSystems systems)
        {
            var poolInputTick   = _inputWorld.GetPool<InputTickComponent>();
            var poolInput  = _inputWorld.GetPool<InputComponent>();

            var tick = _world.GetTick();
            
            foreach (var inputEntity in _filter)
            {
                if (poolInputTick.GetNullable(inputEntity)?.Tick != tick)
                    continue;

                var inputType = poolInput.Get(inputEntity).Type;

                if (inputType == typeof(InputBeginMouseDragComponent))
                {
                    var data = inputEntity.EntityGet<InputBeginMouseDragComponent>(_inputWorld);
                    if (data.Entity.Unpack(_world, out int entity))
                    {
                        int iiii = 0;
                    }
                    continue;
                }
                
                if (inputType == typeof(InputUpdateMouseDragComponent))
                {
                    var data = inputEntity.EntityGet<InputUpdateMouseDragComponent>(_inputWorld);
                    /*
                    if (data.Entity.Unpack(_world, out int entity))
                    {
                        int iiii = 0;
                    }*/
                    continue;
                }
            }
        }
    }
}
