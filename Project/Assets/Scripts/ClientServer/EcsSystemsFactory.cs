﻿using Fabros.Ecs.ClientServer.Serializer;
using Fabros.Ecs.ClientServer.Systems;
using Fabros.EcsModules.Box2D.ClientServer.Components;
using Fabros.EcsModules.Box2D.ClientServer.Components.Other;
using Fabros.EcsModules.Box2D.ClientServer.Systems;
using Fabros.EcsModules.Grid;
using Fabros.EcsModules.Tick;
using Game.Ecs.Client.Systems;
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
using Game.Ecs.Client.Systems.Inventory;
using Game.Fabros.EcsModules.Fire.Client.Systems;
using Fabros.EcsModules.Box2D.Client.Systems;
#endif

namespace Game.ClientServer
{
    public class EcsSystemsFactory : IEcsSystemsFactory
    {
        private ComponentsPool pool;
        public EcsSystemsFactory(ComponentsPool pool) 
        {
            this.pool = pool;
        }

        public void AddNewSystems(EcsSystems systems, IEcsSystemsFactory.Settings settings)
        {
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
            
            
            systems.DelHere<Box2DBeginContactComponent>();
            systems.DelHere<Box2DEndContactComponent>();
            systems.DelHere<Box2DPreSolveComponent>();
            systems.DelHere<Box2DPostSolveComponent>();
            
            
            systems.Add(new EntitiesLifeTimeSystem());
            
            systems.Add(GridModule.GetSystems());
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

#if CLIENT
            AddClient(new UseInventoryItemSystem());
#endif

            systems.Add(new AddInventoryItemSystem());
            systems.Add(new RemoveInventoryItemSystem());

            systems.Add(new FireSystem());

            systems.Add(new ApplyForceSystem());
            systems.Add(new ShotSystem());

#if CLIENT
            AddClient(new CreateViewSystem());
            AddClient(new FireViewSystem());
            AddClient(new InventorySystem());
#endif

            systems.Add(new DeleteEntitiesSystemWith<DestroyComponent>());
            systems.DelHere<InputComponent>("input");
            systems.Add(new DeleteEntitiesSystemWith<EventComponent>());


            systems.Add(new FireDestroyEntitySystem());

            AddServer(new ObjectivesSystem());

            systems.Add(TickModule.GetSystems());
            
#if CLIENT
            AddClient(new AnimateCharacterSystem());
#endif

            systems.Add(new AddLerpSystem());

            

            
            systems.Add(new Box2DSystem(Config.POSITION_ITERATIONS, Config.VELOCITY_ITERATIONS, 
                new Vector2(0,0)));
            
            
            systems.Add(new EventsSystem<Box2DBodyComponent>());
            systems.Add(new EventsSystem<FireComponent>());
            systems.Add(new EventsSystem<ButtonPressedComponent>());
            systems.Add(new EventsSystem<PlayerComponent>());
            systems.Add(new EventsSystem<ButtonPushCompleted>());
            systems.Add(new EventsSystem<ObjectiveOpenedComponent>());
            systems.Add(new EventsSystem<ObjectiveCompletedComponent>());
            systems.Add(new EventsSystem<GateOpenedComponent>());
            systems.Add(new EventsSystem<FoodCollectedComponent>());
            systems.Add(new EventsSystem<PushingComponent>());
            systems.Add(new EventsSystem<MakeShotComponent>());
            
#if CLIENT
            systems.Add(new EventsSystem<AnimationStateComponent>());
            systems.Add(new EventsSystem<MovingComponent>());
#endif


            //write final Box2d transforms to components
            systems.Add(new Box2DWriteStateToComponentsSystem());
        }
    }
}