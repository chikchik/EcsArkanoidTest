using UnityEngine;

namespace Game.Ecs.ClientServer.Components.Input
{
    
    public interface IInputComponent
    {
    }

    
    public struct InputActionComponent
    {
    }
    
    public struct InputShotComponent:IInputComponent
    {
        public Vector3 dir;
    }
}