using System;
using UnityEngine;

namespace Game.UI.FSM
{
    [Serializable]
    public class FSMState
    {
        public FSM fsm;

        public FSMState(FSM fsm)
        {
            this.fsm = fsm;
        }

        public virtual void Enter()
        {
            Debug.Log($"State Enter {this}");
        }
        
    
        public virtual void Exit()
        {
            Debug.Log($"State Exit {this}");
        }
        
        public virtual void Pause()
        {
          
        }

        public virtual void Resume()
        {
        }


        public virtual void Tick()
        {
        }
        

        public virtual void Update()
        {
        }
    }
}