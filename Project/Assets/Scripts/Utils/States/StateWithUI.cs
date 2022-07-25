using Game.Utils.States.FSM;
using Game.View;

namespace Game.Utils.States
{
    public class StateWithUI : FSMState
    {
        public StateWithUI(FSM.FSM fsm) : base(fsm) 
        {
        }
    }

    public class StateWithUI<T> : StateWithUI where T : BaseUIView
    {
        protected T view;

        private bool isInitialized;
    
        public StateWithUI(FSM.FSM fsm) : base(fsm) 
        {
        }

        public sealed override void Enter()
        {
            Initialize();
            base.Enter();
            view.Show();
        
            DoEnter();
        }

        public T GetView()
        {
            return view;
        }
    
    

        protected virtual void DoEnter()
        {
        
        }
    
        public sealed override void Exit()
        {
            base.Exit();
            view.Hide();
            DoExit();
        }
    
        protected virtual void DoExit()
        {
        
        }

        private void Initialize()
        {
            if (isInitialized)
                return;
            isInitialized = true;
            DoInitialize();
        }

        protected virtual void DoInitialize()
        {
        
        }

        protected void Close()
        {
            fsm.PopThis(this);
        }
    }
}