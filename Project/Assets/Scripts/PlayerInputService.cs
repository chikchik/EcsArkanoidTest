using System;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Fabros.Net.ClientServer;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Game
{
    public class PlayerInputService
    {
        private EcsWorld inputWorld;
        private EcsWorld world;

        private IInputService input;
        
        public PlayerInputService(
            [Inject(Id = "input")] EcsWorld inputWorld, 
            EcsWorld world)
        {
            this.inputWorld = inputWorld;
            this.world = world;
        }

        public void SetInputService(IInputService inputService)
        {
            this.input = inputService;
        }
        

        private int playerId => world.GetUnique<MainPlayerIdComponent>().value;
        private int unitEntity => BaseServices.GetUnitEntityByPlayerId(world, playerId);
        
        public void Shot()
        {
            var component = new InputShotComponent();
            component.dir = world.EntityGet<LookDirectionComponent>(unitEntity).value;
            input.Input(inputWorld, playerId, component);
        }
        
        public void MoveToDirection(Vector3 dir)
        {
            var component = new InputMoveDirectionComponent();
            component.Dir = dir;
            input.Input(inputWorld, playerId, component);
        }
        
        public void StopMoveToDirection()
        {
            if (unitEntity == -1)
                return;
            
            if (!unitEntity.EntityHas<MoveDirectionComponent>(world))
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Dir = Vector3.zero;
            input.Input(inputWorld, playerId, component);
        }
        
        public void MoveToPoint(Vector3 pos)
        {
            var component = new InputMoveToPointComponent();
            component.Value = pos;
            input.Input(inputWorld, playerId, component);
        }
    }
}