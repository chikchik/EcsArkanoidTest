using System;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Input.Proto;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerControlService
    {
        private EcsWorld inputWorld;
        private EcsWorld world;

        private IInputService input;
        
        public PlayerControlService(
            [Inject(Id = "input")] EcsWorld inputWorld, 
            EcsWorld world,
            IInputService input
            )
        {
            this.inputWorld = inputWorld;
            this.world = world;
            this.input = input;
        }


        private int playerId => world.GetUnique<MainPlayerIdComponent>().value;
        private int unitEntity => BaseServices.GetUnitEntityByPlayerId(world, playerId);
        private int tick => world.GetTick() + 1;//next tick
        
        public void Shot()
        {
            var component = new InputShotComponent();
            component.dir = world.EntityGet<LookDirectionComponent>(unitEntity).value;
            
            Apply(component);
        }
        
        public void MoveToDirection(Vector3 dir)
        {
            var component = new InputMoveDirectionComponent();
            component.Dir = dir;
            
            Apply(component);
        }
        
        public void StopMoveToDirection()
        {
            if (unitEntity == -1)
                return;
            
            if (!unitEntity.EntityHas<MoveDirectionComponent>(world))
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Dir = Vector3.zero;
            
            Apply(component);
        }
        
        public void MoveToPoint(Vector3 pos)
        {
            var component = new InputMoveToPointComponent();
            component.Value = pos;
            Apply(component);
        }

        private void Apply(IInputComponent component)
        {
            var inputEntity = inputWorld.NewEntity();
            inputEntity.EntityAdd<InputComponent>(inputWorld);
            inputEntity.EntityAdd<InputTickComponent>(inputWorld).Tick = tick;
            inputEntity.EntityAdd<InputPlayerComponent>(inputWorld).PlayerID = playerId;
            
            var pool = inputWorld.GetOrCreatePoolByType(component.GetType());
            pool.AddRaw(inputEntity, component);
            
            input.Input(inputWorld, playerId, tick, component);
        }
    }
}