using Fabros.Ecs.ClientServer.Systems;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Grid;
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
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
using UnityEngine;

#if CLIENT
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Game.Fabros.EcsModules.Fire.Client.Systems;
using Fabros.EcsModules.Box2D.Client.Systems;
#endif

namespace Game.ClientServer
{
    public class EcsSystemsFactory : IEcsSystemsFactory
    {
        private ComponentsCollection pool;
        public EcsSystemsFactory(ComponentsCollection pool) 
        {
            this.pool = pool;
        }

        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
            systems.Add(new DebugMeSystem(true));
            
            var client = settings.client;
            var server = settings.server;

#if CLIENT
            void AddClient(IEcsSystem system)
            {
                //иногда на клиенте нужно получить список систем
                //куда не входят по настоящему клиентские системы (привязанные к Unity) 
                if (client)
                    systems.Add(system);
            }
#endif

            void AddServer(IEcsSystem system)
            { 
                //для сингл плеера нужны все игровые системы
                if (server)
                    systems.Add(system);
            }
       
            
#if SERVER
            systems.Add(new CreateGameSystem(pool));
#endif
            
#if CLIENT
            AddClient(new DetectPlayerIdChangesSystem());
#endif
            
            //AddServer(new SpawnBotSystem());
            AddServer(new JoinPlayerSystem());


            //systems.Add(new ApplyInput0System());
            
#if CLIENT
            AddClient(new InitSceneSystem());
#endif
            

            
#if CLIENT
            AddClient(new Box2DDebugViewSystem());
#endif
            
            AddServer(new AIPlayerSystem());

            systems.Add(new MoveToTargetPositionSystem());
            systems.Add(new MoveSystem());
            systems.Add(new LookDirectionSystem());
            systems.Add(new SimpleMoveSystem());
            systems.Add(new UnitMoveSystem());
            systems.Add(new PushingSystem());
            
            
            systems.Add(new EntitiesLifeTimeSystem());
            
            systems.Add(GridModule.GetSystems());
            
            systems.DeleteComponentHere<ShootStartedComponent>();//если ставить в конец, то на клиент этот компонент даже придет
            systems.Add(new ApplyInputSystem());


            AddServer(new FootprintSystem());

#if CLIENT
            AddClient(new FootprintViewSystem());
            AddClient(new HighlightInteractableSystem());
#endif
            //systems.Add(new BushInteractionSystem());
            //systems.Add(new BoxInteractionSystem());

#if CLIENT
            AddClient(new CollectableSystem());
#endif
            // gates and buttons
            systems.Add(new ButtonsInteractionSystem());
            AddServer(new ButtonCustomSystem());
            systems.Add(new GateSystem());
            systems.Add(new MoveByProgressSystem());
            
            systems.Add(new FireSystem());

            systems.Add(new ApplyForceSystem());
            
            

#if CLIENT
            AddClient(new CreateViewSystem());
            AddClient(new FireViewSystem());
            AddClient(new InventorySystem());
#endif

            systems.Add(new DeleteEntitiesSystemWith<DestroyComponent>());

            AddServer(new DeleteInputEntitiesSystem());
            AddServer(new DeleteOutdatedInputEntitiesSystem());

            systems.Add(new FireDestroyEntitySystem());

            AddServer(new ObjectivesSystem());

            systems.Add(new DestroyAtTimeSystem());

            
#if CLIENT
            AddClient(new CharacterAnimationSystem());
#endif

            systems.Add(new AddLerpSystem());

            
            //Основная Box2dSystem должна быть в конце после всех основных систем,
            //иначе в мультиплеере предсказание не будет работать правильно
            systems.Add(new Box2DInitSystem());
            systems.Add(new Box2DCreateBodiesSystem());
            //systems.Add(new Box2DCreateContactsSystem());
            systems.Add(new Box2DUpdateInternalObjectsSystem());
            systems.Add(new Box2DUpdateSystem(Config.POSITION_ITERATIONS, Config.VELOCITY_ITERATIONS));
            systems.Add(new BulletContactSystem());      
            
            systems.Add(new ShootSystem());

            systems.Add(new Box2DDeleteContactsSystem());
            
            systems.Add(new EventsSystem<FireComponent>());
            systems.Add(new EventsSystem<ButtonPressedComponent>());
            systems.Add(new EventsSystem<PlayerComponent>());
            systems.Add(new EventsSystem<ButtonPushCompleted>());
            systems.Add(new EventsSystem<ObjectiveOpenedComponent>());
            systems.Add(new EventsSystem<ObjectiveCompletedComponent>());
            systems.Add(new EventsSystem<GateOpenedComponent>());
            systems.Add(new EventsSystem<FoodCollectedComponent>());
            systems.Add(new EventsSystem<PushingComponent>());
            systems.Add(new EventsSystem<CantMoveComponent>());
            
#if CLIENT
            systems.Add(new EventsSystem<MovingComponent>());
#endif
            //write final Box2d transforms to components
            systems.Add(new Box2DWriteBodiesToComponentsSystem());
            
            systems.Add(new DebugMeSystem(false));
            
            //тик меняется на следующий в самом конце после всех систем 
            systems.Add(TickModule.GetSystems());
        }
    }
}