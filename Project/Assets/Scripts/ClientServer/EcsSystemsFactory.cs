using Game.ClientServer.Services;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Systems;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Systems;
using XFlow.EcsLite;
using XFlow.Modules.Box2D.ClientServer.Systems;
using XFlow.Modules.Fire.ClientServer.Components;
using XFlow.Modules.Fire.ClientServer.Systems;
using XFlow.Modules.Grid.Systems;
using XFlow.Modules.Inventory.ClientServer.Systems;
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
        private EcsSystemsContainer _container;
        
        public EcsSystemsFactory(DiContainer di)
        {
            _container = new EcsSystemsContainer(di);
            
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
            
            /*
             ПРИМЕРЫ
             
             container.RegisterServer<JoinPlayerSystem>();
             Система создается только на сервере и вход игрока может быть произведен только на сервере, никак не стартует в юнити
             
             

             container.RegisterServer<AIPlayerSystem>();
             АИ бота, который ходит по случ. точкам работает отрабатывает только на сервере
             
             container.Register<GateSystem>();
             система открытия ворот нужна везде, чтоб клиент мог на опережение начинать открывать ворота локально
             а позже результат ее работы долетит и с сервера
             
              
             container.RegisterClient<CreateViewSystem>();
             любые системы которые создают View, а значит чисто клиентские регаются через RegisterClient
             
             
             container.RegisterClient<AddLerpSystem>();
             Эта система успешно может работать и на сервере, но там она не имеет смысла, потому что она добавляет
              LerpComponent для текущего игрока,а текущий игрок существует только у клиента, для сервера все равны
              
              
              container.RegisterClient<DetectPlayerIdChangesSystem>();     
              чисто клиентская система, которая дожидается что сервер создаст для нашего игрока сущность и пришлет нам        
              
             */
            
            /*
             #if SERVER - Определено на сервере        
                               
             нужны только для того чтобы избежать ошибок компиляции, 
             на сервере, например, физически нет систем, завязанных на GameObject
             потому даже их упоминания нужно "физически" исключить из кода
             
             #if CLIENT - Определено в юнити клиенте
             в большинстве случае не влияет, потому что там все равно внутри RegisterClient
             и даже без #if CLIENT сервер эти системы не будет создавать                         
                           
             */
#if SERVER
            _container.Register<CreateGameSystem>();
#endif
            
#if CLIENT
            _container.RegisterClient<DetectPlayerIdChangesSystem>();
#endif
            _container.RegisterServer<CustomInitSystem>();
            _container.RegisterServer<JoinPlayerSystem>();
            
#if CLIENT
            _container.RegisterClient<InitSceneSystem>();
#endif
            
            _container.RegisterServer<AIPlayerSystem>();

            _container.Register<MoveToTargetPositionSystem>();
            _container.Register<MoveSystem>();
            _container.Register<LookDirectionSystem>();
            _container.Register<SimpleMoveSystem>();
            _container.Register<UnitMoveSystem>();
            _container.Register<FollowSystem>();
            _container.Register<PushingSystem>();

            _container.Register<MechAdapterSystem>();
            
            _container.Register<EntitiesLifeTimeSystem>();
            
            _container.Register<GridSystem>();
            
            _container.Register<DeleteComponentHereSystem<ShootStartedComponent>>();//если ставить в конец, то на клиент этот компонент даже придет
            
            _container.Register<ApplyInputSystem>();


            _container.RegisterServer<FootprintSystem>();

#if CLIENT
            _container.RegisterClient<FootprintViewSystem>();
            _container.RegisterClient<HighlightInteractableSystem>();
            _container.RegisterClient<VFXCreationSystem>();
#endif
            

#if CLIENT
            _container.RegisterClient<CollectableSystem>();
            _container.RegisterClient<WeaponEquipSystem>();
#endif
            // gates and buttons
            _container.Register<ButtonsInteractionSystem>();
            _container.RegisterServer<ButtonCustomSystem>();
            _container.Register<GateSystem>();
            _container.Register<MoveByProgressSystem>();

            _container.Register<FireSystem>();

            _container.Register<ApplyForceSystem>();
            
            

#if CLIENT
            _container.RegisterClient<CreateViewSystem>();
            _container.RegisterClient<FireViewSystem>();
            _container.RegisterClient<InventoryImageSystem>();
            _container.RegisterClient<CreateMechViewSystem>();
            _container.RegisterClient<MechAnimationSystem>();
#endif

            _container.Register<DeleteEntityHereSystem<DestroyComponent>>();

            _container.RegisterServer<DeleteInputEntitiesSystem>();
            _container.RegisterServer<DeleteOutdatedInputEntitiesSystem>();

            _container.Register<FireDestroyEntitySystem>();

            _container.RegisterServer<ObjectivesSystem>();

            _container.Register<DestroyAtTimeSystem>();

#if CLIENT
            _container.RegisterClient<CharacterAnimationSystem>();
            _container.RegisterClient<AddLerpSystem>();
#endif


            _container.Register<ShootSystem>();
            
            //Основная Box2dSystem должна быть в конце после всех основных систем,
            //иначе в мультиплеере предсказание не будет работать правильно
            _container.Register<Box2DInitSystem>();
            _container.Register<Box2DCreateBodiesSystem>();
            _container.Register<Box2DCreateEventContactsSystem>();
            //container.Register<Box2DCreateContactsSystem>();
            _container.Register<Box2DUpdateInternalObjectsSystem>();
            _container.Register<Box2DUpdateSystem>();
            _container.Register<BulletContactSystem>();      
            _container.Register<DestructibleDamageApplySystem>();

            //container.Register<Box2DDeleteContactsSystem>();

            _container.Register<EventsSystem<WeaponComponent>>();
            _container.Register<EventsSystem<FireComponent>>();
            _container.Register<EventsSystem<ButtonPressedComponent>>();
            _container.Register<EventsSystem<PlayerComponent>>();
            _container.Register<EventsSystem<ButtonPushCompleted>>();
            _container.Register<EventsSystem<ObjectiveOpenedComponent>>();
            _container.Register<EventsSystem<ObjectiveCompletedComponent>>();
            _container.Register<EventsSystem<GateOpenedComponent>>();
            _container.Register<EventsSystem<FoodCollectedComponent>>();
            _container.Register<EventsSystem<AmmoCollectedComponent>>();
            _container.Register<EventsSystem<PushingComponent>>();
            _container.Register<EventsSystem<CantMoveComponent>>();
            
#if CLIENT
            _container.Register<EventsSystem<MovingComponent>>();
            _container.Register<EventsSystem<InventorySlotComponent>>();
            _container.Register<EventsSystem<ActiveInventoryCategoryComponent>>();
            _container.Register<EventsSystem<EntityRemoveEventComponent>>();
            _container.Register<EventsSystem<TickComponent>>();
            _container.Register<EventsSystem<ControlsMechComponent>>();
            _container.Register<EventsSystem<HpComponent>>();
            _container.RegisterClient<CreateViewSystem>();
            _container.RegisterClient<Box2DDebugViewSystem>();
            _container.RegisterClient<BulletDestroyFxSystem>();
            
            _container.RegisterClient<DeadWorldClearSystem>();
#endif
            
            _container.Register<DeleteEventEntitiesSystem>();
            _container.Register<RemoveEntitySystem>();//from inventory

            //write final Box2d transforms to components
            _container.Register<Box2DWriteBodiesToComponentsSystem>();
        }
        
        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
            systems.Add(_container.CreateNewSystems(systems.GetWorld(), settings.AddClientSystems, settings.AddServerSystems));
        }

        public IEcsSystem CreateSyncDebugSystem(bool pre)
        {
            return new DebugMeSystem(pre);
        }
    }
}