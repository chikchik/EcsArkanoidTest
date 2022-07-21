using System;
using Fabros.Ecs.Client.Components;
using Fabros.Ecs.ClientServer.Components;
using Fabros.Ecs.Utils;
using Fabros.EcsModules.Tick.Other;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components;
using Game.Ecs.ClientServer.Components.Input;
using Game.Ecs.ClientServer.Components.Input.proto;
using Game.Fabros.Net.ClientServer;
using Game.Fabros.Net.ClientServer.Ecs.Components;
using Game.Fabros.Net.ClientServer.Utils;
using Game.View;
using Flow.EcsLite;
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
            [InjectOptional] IInputService input,
            EcsWorld world)
        {
            this.inputWorld = inputWorld;
            this.world = world;
            this.input = input;
        }
        
        private int playerId => world.GetUnique<MainPlayerIdComponent>().value;
        private int unitEntity => BaseServices.GetUnitEntityByPlayerId(world, playerId);
        private int tick => world.GetTick();
        
        public void Shot()
        {
            if (unitEntity == -1)
                return;

            var view = unitEntity.EntityGet<TransformComponent>(world).Transform.GetComponent<CharacterView>();
            //view.
            
            var component = new InputShotComponent();
            var lookDir = world.EntityGet<LookDirectionComponent>(unitEntity).value;
            var dir = Quaternion.Euler(0, -22, 0) * lookDir;
            component.dir = dir;
            component.pos = view.BulletSpawnPos.transform.position; 
            
            Apply(component);
        }
        
        public void Interact()
        {
            if (unitEntity == -1)
                return;
            
            var component = new InputActionComponent();
            Apply(component);
        }
        
        public void Kick()
        {
            if (unitEntity == -1)
                return;
            
            var component = new InputKickComponent();
            component.dir = world.EntityGet<LookDirectionComponent>(unitEntity).value;
            
            Apply(component);
        }
        
        public void Mech()
        {
            if (unitEntity == -1)
                return;

            Apply(new InputMechEnterLeaveComponent());
        }
        
        public void MoveToDirection(Vector3 dir)
        {
            if (unitEntity == -1)
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Dir = dir;
            
            Apply(component);
        }
        
        public void StopMoveToDirection()
        {
            if (unitEntity == -1)
                return;

            var entity = ApplyInputWorldService.GetControlledEntity(world, unitEntity);
            
            if (!entity.EntityHas<MoveDirectionComponent>(world))
                return;
            
            var component = new InputMoveDirectionComponent();
            component.Dir = Vector3.zero;
            
            Apply(component);
        }
        
        public void MoveToPoint(Vector3 pos)
        {
            if (unitEntity == -1)
                return;
            
            var component = new InputMoveToPointComponent();
            component.Value = pos;
            Apply(component);
        }

        private void Apply(IInputComponent component)
        {
            //reused on server
            ApplyInputWorldService.CreateInputEntity(inputWorld, playerId, tick, component);
            
            //send player input to server
            input?.Input(inputWorld, playerId, tick, component);
        }
    }
}