using Fabros.Ecs;
using Fabros.Ecs.Systems;
using Fabros.EcsModules.Base.Components;
using Fabros.EcsModules.Grid;
using Fabros.EcsModules.Tick;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Events;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Ecs.ClientServer.Systems;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Game.Fabros.EcsModules.Fire.ClientServer.Systems;
using Leopotam.EcsLite;
using Leopotam.EcsLite.ExtendedSystems;
#if CLIENT
using Game.Ecs.Client.Systems;
using Game.Ecs.Client.Systems.Inventory;
using Game.Fabros.EcsModules.Fire.Client.Systems;
using Game.Fabros.Net.Client;
#endif

namespace Game.ClientServer
{
    public class SystemsAndComponents
    {
        //public LeoContexts leo { get; set; }

        public static ComponentsPool CreateComponentsPool()
        {
            /*
             * общий код для клиента и сервера
             * ComponentsPool необходим, чтобы компоненты правильно передавались через сеть
             * если компонента в списке нет, то он не будет попадать в diff
            */
#if CLIENT
            var pool = new ComponentsPool(new UnityJsonComponentSerializer());
#else
            var pool = new ComponentsPool(new JsonComponentSerializer());
#endif

            pool.AddComponent<MoveDirectionComponent>();
            pool.AddComponent<PlayerComponent>();
            pool.AddComponent<TickrateConfigComponent>();

            TickModule.AddSerializableComponents(pool);
            GridModule.AddSerializableComponents(pool);

            pool.AddComponent<FireComponent>();
            pool.AddComponent<BurnedOutComponent>();
            pool.AddComponent<FlammableComponent>();

            pool.AddComponent<GameObjectNameComponent>();
            pool.AddComponent<BoxComponent>();
            pool.AddComponent<BushComponent>();
            pool.AddComponent<ButtonComponent>();
            pool.AddComponent<ButtonLinkComponent>();
            pool.AddComponent<UnitComponent>();
            pool.AddComponent<GateComponent>();
            pool.AddComponent<MoveInfoComponent>();

            pool.AddComponent<HealthComponent>();
            pool.AddComponent<InteractableComponent>();
            pool.AddComponent<CollectableComponent>();
            pool.AddComponent<TargetPositionComponent>();
            pool.AddComponent<ProgressComponent>();
            pool.AddComponent<PositionComponent>();
            pool.AddComponent<RadiusComponent>();
            pool.AddComponent<ResourceComponent>();
            pool.AddComponent<SpeedComponent>();

            pool.AddComponent<InventoryItemComponent>();
            pool.AddComponent<InventorySlotComponent>();

            pool.AddComponent<FootprintComponent>();
            pool.AddComponent<LastFootprintComponent>();
            pool.AddComponent<LifeTimeComponent>();
            pool.AddComponent<AIPlayerComponent>();
            pool.AddComponent<DestroyComponent>();


            pool.AddComponent<ObjectiveCompletedComponent>();
            pool.AddComponent<ObjectiveOpenedComponent>();
            pool.AddComponent<ObjectiveComponent>();
            pool.AddComponent<ObjectiveDescriptionComponent>();

            pool.AddComponent<ButtonPushCompleted>();
            pool.AddComponent<GateOpenedComponent>();

            return pool;
        }

        public static void AddSystems(ComponentsPool pool, EcsSystems systems, bool client)
        {
#if CLIENT
            void AddClient(IEcsSystem system)
            {
                //иногда на клиенте нужно получить список систем
                //куда не входят по настоящему клиентские системы (привязанные к Unity) 
                if (client)
                    systems.Add(system);
            }
#endif


#if SERVER
            systems.Add(new CreateGameSystem(pool));
            systems.Add(new SpawnBotSystem());
            systems.Add(new JoinPlayerSystem());
#else
            AddClient(new InitSceneSystem());
#endif

            systems.Add(new ApplyUserInputSystem());

#if SERVER
            systems.Add(new AIPlayerSystem());
#endif

            systems.Add(new MoveToTargetPositionSystem());
            systems.Add(new MoveSystem());
            systems.Add(new EntitiesLifeTimeSystem());

#if CLIENT
            AddClient(new RotateCharacterSystem());
            AddClient(new AnimateCharacterSystem());
#endif
            systems.Add(GridModule.GetSystems());

            // footprint
            systems.Add(new FootprintSystem());

#if CLIENT
            AddClient(new FootprintViewSystem());
#endif


            // interaction
            systems.Add(new InteractionSystem());
#if CLIENT
            AddClient(new HighlightInteractableSystem());
#endif
            systems.Add(new BushInteractionSystem());
            systems.Add(new BoxInteractionSystem());
            systems.Add(new BurnInteractionSystem());

#if CLIENT
            AddClient(new CollectableSystem());
#endif
            // gates and buttons
            systems.Add(new ButtonActivationSystem());
            systems.Add(new GateSystem());
            systems.Add(new MoveByProgressSystem());

#if CLIENT
            AddClient(new SyncTransformSystem());
            AddClient(new CameraFollowSystem());
            AddClient(new UseInventoryItemSystem());
            AddClient(new DetectPlayerIdChangesSystem());
#endif

            systems.Add(new AddInventoryItemSystem());
            systems.Add(new RemoveInventoryItemSystem());

            systems.Add(new FireSystem());

#if CLIENT
            AddClient(new FireViewSystem());
            AddClient(new InventorySystem());
#endif

            systems.Add(new DeleteEntitiesSystemWith<DestroyComponent>());
            systems.DelHere<InputComponent>("input");
            systems.Add(new DeleteEntitiesSystemWith<EventComponent>());


            systems.Add(new FireDestroyEntitySystem());

#if SERVER
            systems.Add(new ObjectivesSystem());
#endif

            systems.Add(TickModule.GetSystems());


            systems.Add(new EventsSystem<PlayerComponent>());
            systems.Add(new EventsSystem<ButtonPushCompleted>());
            systems.Add(new EventsSystem<ObjectiveOpenedComponent>());
            systems.Add(new EventsSystem<ObjectiveCompletedComponent>());
            systems.Add(new EventsSystem<GateOpenedComponent>());
        }
    }
}