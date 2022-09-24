using Game.ClientServer;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.EcsLite;
using XFlow.Modules.Tick.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Systems;
using XFlow.Modules.Tick.Other;
using XFlow.Net.Client;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;
using Zenject;

namespace Game
{
    public class SingleGame
    {
        [Inject] private EcsWorld _world;
        [Inject(Id = EcsWorlds.Input)] private EcsWorld _inputWorld;
        [Inject(Id = EcsWorlds.Event)] private EcsWorld _eventWorld;
        [Inject(Id = EcsWorlds.Dead)]  private EcsWorld _deadWorld;
        
        
        [Inject] 
        private IEcsEntityDestroyedListener _entityDestroyedListener;


        [Inject]
        private IEcsSystemsFactory _systemsFactory;
        
        [Inject]
        private IEcsViewSystemsFactory _viewSystemsFactory;


        private EcsSystems _systems;
        private EcsSystems _viewSystems;
        
        public void PreInit()
        {
            UnityEngine.Physics.autoSimulation = false;
            UnityEngine.Physics2D.simulationMode = SimulationMode2D.Script;
            
            
            _systems = new EcsSystems(_world, "systems");
            _systems.AddWorld(_inputWorld, EcsWorlds.Input);
            _systems.AddWorld(_eventWorld, EcsWorlds.Event);
            _systems.AddWorld(_deadWorld, EcsWorlds.Dead);
            
            _systemsFactory.AddNewSystems(_systems, 
                new IEcsSystemsFactory.Settings{AddServerSystems = true});
            _systems.Add(new TickSystem());
            _systems.Add(new DeleteDeadWorldEntitiesSystem());
            
            _systems.PreInit();
            
            
            _world.EntityCreatedListeners.Add(new AllEntitiesAreReliableListener(_world));
            
            _world.EntityDestroyedListeners.Add(_entityDestroyedListener);
            _world.EntityDestroyedListeners.Add(new CopyToDeadWorldListener(_deadWorld));

            _world.AddUnique<PrimaryWorldComponent>();
            
            _world.AddUnique(new TickDeltaComponent
            {
                Value = new TickDelta((int)(1f/Time.fixedDeltaTime))
            });

            _world.AddUnique(new TickComponent{Value = new Tick(0)});
           
            
            _viewSystems = new EcsSystems(_world, "viewSystems");
            _viewSystems.AddWorld(_deadWorld, EcsWorlds.Dead);
            
            _viewSystemsFactory.AddNewSystems(_viewSystems);
            
            // в мультиплеере это контролируется на стороне сервера
            _viewSystems.Add(new DeleteDestroyedEntitiesSystem());
            _viewSystems.PreInit();
        }

        public void Init()
        {
            
            _systems.Init();
            _viewSystems.Init();
        }

        public void Update()
        {
            _viewSystems.Run();
        }
        
        public void FixedUpdate()
        {
            _systems.Run();
        }
    }
}