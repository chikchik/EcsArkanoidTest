using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Systems;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Fire.ClientServer.Systems;
using XFlow.Modules.Grid.Systems;
using XFlow.Net.ClientServer;
using XFlow.Net.ClientServer.Ecs.Components;
using XFlow.Net.ClientServer.Ecs.Systems;

#if CLIENT
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Fabros.EcsModules.Mech.Client.Systems;
using XFlow.Modules.Box2D.Client.Systems;
using XFlow.Modules.Fire.Client.Systems;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Modules.Tick.ClientServer.Components;
#endif

using Zenject;

namespace Game.ClientServer
{
    public class EcsSystemsFactory : IEcsSystemsFactory
    {
        private EcsSystemsContainer container;
        
        public EcsSystemsFactory(DiContainer di)
        {
            container = new EcsSystemsContainer(di);
            
            /*
             * регистрация аналогична прямым вызовам через systems.AddSystem, за тем исключением,
             * что система будет создана через Zenject 
             */
            
            /*
             * системы регистрируются в порядке следования
             * RegisterClient - система, которая будет отрабатывать в мире где есть Unity Client
             * RegisterServer - система, которая будет создана и на стороне сервера (или синглплеера)
             * Register - система, регистрируется для всех случаев
             */
#if SERVER
            container.Register<CreateGameSystem>();
#endif
            
#if CLIENT
            container.RegisterClient<DetectPlayerIdChangesSystem>();
#endif
            
            container.RegisterServer<CustomInitSystem>();
            container.RegisterServer<JoinPlayerSystem>();
            
#if CLIENT
            container.RegisterClient<InitSceneSystem>();
            container.RegisterClient<Box2DDebugViewSystem>();
#endif
            
            container.RegisterServer<AIPlayerSystem>();

            container.Register<MoveToTargetPositionSystem>();
            container.Register<MoveSystem>();
            container.Register<LookDirectionSystem>();
            container.Register<SimpleMoveSystem>();
            container.Register<UnitMoveSystem>();
            container.Register<PushingSystem>();

            container.Register<MechAdapterSystem>();
            
            container.Register<EntitiesLifeTimeSystem>();
            
            container.Register<GridSystem>();
            
            container.Register<DeleteComponentHereSystem<ShootStartedComponent>>();//если ставить в конец, то на клиент этот компонент даже придет
            
            container.Register<ApplyInputSystem>();


            container.RegisterServer<FootprintSystem>();

#if CLIENT
            container.RegisterClient<FootprintViewSystem>();
            container.RegisterClient<HighlightInteractableSystem>();
#endif
            

#if CLIENT
            container.RegisterClient<CollectableSystem>();
            container.RegisterClient<WeaponEquipSystem>();
#endif
            // gates and buttons
            container.Register<ButtonsInteractionSystem>();
            container.RegisterServer<ButtonCustomSystem>();
            container.Register<GateSystem>();
            container.Register<MoveByProgressSystem>();
            
            container.Register<FireSystem>();

            container.Register<ApplyForceSystem>();
            
            

#if CLIENT
            container.RegisterClient<CreateViewSystem>();
            container.RegisterClient<FireViewSystem>();
            container.RegisterClient<InventoryImageSystem>();
            container.RegisterClient<CreateMechViewSystem>();
            container.RegisterClient<MechAnimationSystem>();
#endif

            container.Register<DeleteEntityHereSystem<DestroyComponent>>();

            container.RegisterServer<DeleteInputEntitiesSystem>();
            container.RegisterServer<DeleteOutdatedInputEntitiesSystem>();

            container.Register<FireDestroyEntitySystem>();

            container.RegisterServer<ObjectivesSystem>();

            container.Register<DestroyAtTimeSystem>();

#if CLIENT
            container.RegisterClient<CharacterAnimationSystem>();
            container.RegisterClient<AddLerpSystem>();
#endif


            container.Register<ShootSystem>();
            
            //Основная Box2dSystem должна быть в конце после всех основных систем,
            //иначе в мультиплеере предсказание не будет работать правильно
            container.Register<Box2DInitSystem>();
            container.Register<Box2DCreateBodiesSystem>();
            container.Register<Box2DCreateContactsSystem>();
            container.Register<Box2DUpdateInternalObjectsSystem>();
            container.Register<Box2DUpdateSystem>();
            container.Register<BulletContactSystem>();
            
            

            container.Register<Box2DDeleteContactsSystem>();
            
            container.Register<EventsSystem<WeaponComponent>>();
            container.Register<EventsSystem<FireComponent>>();
            container.Register<EventsSystem<ButtonPressedComponent>>();
            container.Register<EventsSystem<PlayerComponent>>();
            container.Register<EventsSystem<ButtonPushCompleted>>();
            container.Register<EventsSystem<ObjectiveOpenedComponent>>();
            container.Register<EventsSystem<ObjectiveCompletedComponent>>();
            container.Register<EventsSystem<GateOpenedComponent>>();
            container.Register<EventsSystem<FoodCollectedComponent>>();
            container.Register<EventsSystem<PushingComponent>>();
            container.Register<EventsSystem<CantMoveComponent>>();
            
#if CLIENT
            container.Register<EventsSystem<MovingComponent>>();
            container.Register<EventsSystem<InventorySlotComponent>>();
            container.Register<EventsSystem<ActiveInventoryCategoryComponent>>();
            container.Register<EventsSystem<TickComponent>>();
            container.Register<EventsSystem<ControlsMechComponent>>();
            container.RegisterClient<CreateViewSystem>();
#endif
            //write final Box2d transforms to components
            container.Register<Box2DWriteBodiesToComponentsSystem>();
        }
        
        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
            systems.Add(container.CreateNewSystems(systems.GetWorld(), settings.AddClientSystems, settings.AddServerSystems));
        }

        public IEcsSystem CreateSyncDebugSystem(bool pre)
        {
            return new DebugMeSystem(pre);
        }
    }
}