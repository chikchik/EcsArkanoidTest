using Fabros.Ecs.ClientServer.Systems;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Grid;
using Fabros.EcsModules.Mech.ClientServer;
using Fabros.EcsModules.Tick;

using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Systems;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Systems;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Ecs.Systems;
using Flow.EcsLite;
using Flow.EcsLite.ExtendedSystems;
using UnityEngine;
using System.Collections.Generic;
using System;
using Fabros.EcsModules.Grid.Systems;

#if CLIENT
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Game.Fabros.EcsModules.Fire.Client.Systems;
using Fabros.EcsModules.Box2D.Client.Systems;
using Fabros.EcsModules.Mech.Client.Systems;
using Fabros.EcsModules.Tick.ClientServer.Components;
#endif

using Zenject;

namespace Game.ClientServer
{
    public class EcsSystemsFactory: IEcsSystemsFactory
    {
        public enum Target
        {
            Any,
            Client,
            Server
        }

        public struct RegisteredSystem
        {
            public Type SystemClassType;
            public Target Target;
        }
        
        private List<RegisteredSystem> systems = new List<RegisteredSystem>();
        private DiContainer container;
        
        public EcsSystemsFactory(DiContainer container)
        {
            this.container = container.CreateSubContainer();
            
            /*
             * системы регистрируются в порядке следования
             * RegisterClient - система, которая будет отрабатывать в мире где есть Unity Client
             * RegisterServer - система, которая будет создана и на стороне сервера (или синглплеера)
             * Register - система, регистрируется для всех случаев
             */
            
#if SERVER
            Register<CreateGameSystem>();
#endif
            
#if CLIENT
            RegisterClient<DetectPlayerIdChangesSystem>();
#endif
            
            RegisterServer<CustomInitSystem>();
            RegisterServer<JoinPlayerSystem>();
            
#if CLIENT
            RegisterClient<InitSceneSystem>();
            RegisterClient<Box2DDebugViewSystem>();
#endif
            
            RegisterServer<AIPlayerSystem>();

            Register<MoveToTargetPositionSystem>();
            Register<MoveSystem>();
            Register<LookDirectionSystem>();
            Register<SimpleMoveSystem>();
            Register<UnitMoveSystem>();
            Register<PushingSystem>();

            Register<MechAdapterSystem>();
            
            Register<EntitiesLifeTimeSystem>();
            
            Register<GridSystem>();
            
            Register<DeleteComponentHereSystem<ShootStartedComponent>>();//если ставить в конец, то на клиент этот компонент даже придет
            
            Register<ApplyInputSystem>();


            RegisterServer<FootprintSystem>();

#if CLIENT
            RegisterClient<FootprintViewSystem>();
            RegisterClient<HighlightInteractableSystem>();
#endif
            

#if CLIENT
            RegisterClient<CollectableSystem>();
            RegisterClient<WeaponEquipSystem>();
#endif
            // gates and buttons
            Register<ButtonsInteractionSystem>();
            RegisterServer<ButtonCustomSystem>();
            Register<GateSystem>();
            Register<MoveByProgressSystem>();
            
            Register<FireSystem>();

            Register<ApplyForceSystem>();
            
            

#if CLIENT
            RegisterClient<CreateViewSystem>();
            RegisterClient<FireViewSystem>();
            RegisterClient<InventorySystem>();
            RegisterClient<CreateMechViewSystem>();
            RegisterClient<MechAnimationSystem>();
#endif

            Register<DeleteEntityHereSystem<DestroyComponent>>();

            RegisterServer<DeleteInputEntitiesSystem>();
            RegisterServer<DeleteOutdatedInputEntitiesSystem>();

            Register<FireDestroyEntitySystem>();

            RegisterServer<ObjectivesSystem>();

            Register<DestroyAtTimeSystem>();

            
#if CLIENT
            RegisterClient<CharacterAnimationSystem>();
#endif

            Register<AddLerpSystem>();
            Register<ShootSystem>();
            
            //Основная Box2dSystem должна быть в конце после всех основных систем,
            //иначе в мультиплеере предсказание не будет работать правильно
            Register<Box2DInitSystem>();
            Register<Box2DCreateBodiesSystem>();
            Register<Box2DCreateContactsSystem>();
            Register<Box2DUpdateInternalObjectsSystem>();
            Register<Box2DUpdateSystem>();
            Register<BulletContactSystem>();      
            
            

            Register<Box2DDeleteContactsSystem>();
            
            Register<EventsSystem<WeaponComponent>>();
            Register<EventsSystem<FireComponent>>();
            Register<EventsSystem<ButtonPressedComponent>>();
            Register<EventsSystem<PlayerComponent>>();
            Register<EventsSystem<ButtonPushCompleted>>();
            Register<EventsSystem<ObjectiveOpenedComponent>>();
            Register<EventsSystem<ObjectiveCompletedComponent>>();
            Register<EventsSystem<GateOpenedComponent>>();
            Register<EventsSystem<FoodCollectedComponent>>();
            Register<EventsSystem<PushingComponent>>();
            Register<EventsSystem<CantMoveComponent>>();
            
#if CLIENT
            Register<EventsSystem<MovingComponent>>();
            Register<EventsSystem<TickComponent>>();
            RegisterClient<CreateViewSystem>();
#endif
            //write final Box2d transforms to components
            Register<Box2DWriteBodiesToComponentsSystem>();
        }
        
        private void RegisterClient<T>()
        {
            //иногда на клиенте нужно получить список систем
            //куда не входят по настоящему клиентские системы (привязанные к Unity)
            
            container.Bind<T>().AsTransient();
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Client,
            });
        }
        
        private void RegisterServer<T>()
        {
            container.Bind<T>().AsTransient();
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Server,
            });
        }
        
        private void Register<T>()
        {
            container.Bind<T>().AsTransient();
            
            systems.Add(new RegisteredSystem
            {
                SystemClassType = typeof(T),
                Target = Target.Any
            });
        }
        
        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
            //override global EcsWorld to world from EcsSystems
            container.Rebind<EcsWorld>().ToSelf().FromInstance(systems.GetWorld()).AsTransient();
            
            foreach (var system in this.systems)
            {
                if (system.Target != Target.Any)
                {
                    var add =
                        settings.AddClientSystems && system.Target == Target.Client ||
                        settings.AddServerSystems && system.Target == Target.Server;

                    if (!add)
                        continue;
                }

                var sys = container.Instantiate(system.SystemClassType) as IEcsSystem;
                systems.Add(sys);   
            }
            
        }

        public IEcsSystem CreateSyncDebugSystem(bool pre)
        {
            return new DebugMeSystem(pre);
        }
    }
}