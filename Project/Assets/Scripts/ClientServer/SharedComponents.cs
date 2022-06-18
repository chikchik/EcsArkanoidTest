using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.ClientServer.WorldDiff;
using Fabros.EcsModules.Box2D;
using Fabros.EcsModules.Grid;
using Fabros.EcsModules.Tick;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input.Proto;
using Game.Ecs.ClientServer.Components.Inventory;
using Game.Ecs.ClientServer.Components.Objective;
using Game.Fabros.EcsModules.Fire.ClientServer.Components;
using Game.Fabros.Net.ClientServer.Ecs.Components;

#if CLIENT
using Game.Fabros.Net.Client;
#endif

namespace Game.ClientServer
{
    public class SharedComponents
    {
        //public LeoContexts leo { get; set; }

        public static ComponentsCollection CreateComponentsPool()
        {
            /*
             * общий код для клиента и сервера
             * ComponentsPool необходим, чтобы компоненты правильно передавались через сеть
             * если компонента в списке нет, то он не будет попадать в diff
            */

            var pool = new ComponentsCollection();

            TickModule.AddSerializableComponents(pool);
            GridModule.AddSerializableComponents(pool);
            Box2DModule.AddSerializableComponents(pool);
            
            
            pool.AddComponent<MoveDirectionComponent>();
            pool.AddComponent<PlayerComponent>();
            pool.AddComponent<TickrateConfigComponent>();

            pool.AddComponent<FireComponent>();
            pool.AddComponent<BurnedOutComponent>();
            pool.AddComponent<FlammableComponent>();

            pool.AddComponent<GameObjectNameComponent>();
            pool.AddComponent<BoxComponent>();
            pool.AddComponent<BushComponent>();
            pool.AddComponent<ButtonComponent>();
            pool.AddComponent<ButtonLinkComponent>();
            pool.AddComponent<ButtonPressedComponent>();
            pool.AddComponent<UnitComponent>();
            pool.AddComponent<GateComponent>();
            pool.AddComponent<MoveInfoComponent>();
            
            pool.AddComponent<FoodCollectedComponent>();

            pool.AddComponent<HealthComponent>();
            pool.AddComponent<InteractableComponent>();
            pool.AddComponent<CollectableComponent>();
            pool.AddComponent<TargetPositionComponent>();
            pool.AddComponent<ProgressComponent>();
            pool.AddComponent<PositionComponent>();
            pool.AddComponent<RadiusComponent>();
            pool.AddComponent<ResourceComponent>();
            

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
            pool.AddComponent<LookDirectionComponent>();

           
            pool.AddComponent<ApplyForceComponent>();
            pool.AddComponent<PushingComponent>();


            pool.AddComponent<MoveSimpleDirectionComponent>();
            pool.AddComponent<DestroyWhenTimeIsOutComponent>();
            pool.AddComponent<TimeComponent>();
            pool.AddComponent<StartSimpleMoveAtComponent>();
            
            pool.AddComponent<AverageSpeedComponent>();
            pool.AddComponent<ButtonCustomComponent>();
            
            pool.AddComponent<BulletComponent>();
            pool.AddComponent<MakeShotComponent>();
            
            
            
            pool.AddComponent<PingComponent>();
            pool.AddComponent<InputActionComponent>();
            pool.AddComponent<InputMoveDirectionComponent>();
            pool.AddComponent<InputMoveToPointComponent>();
            pool.AddComponent<InputShotComponent>();

            return pool;
        }

    }
}