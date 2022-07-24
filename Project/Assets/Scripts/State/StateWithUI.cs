using Game.UI.FSM;

public class StateWithUI : FSMState
{
    public StateWithUI(FSM fsm) : base(fsm) 
    {
    }
}

public class StateWithUI<T> : StateWithUI where T : BaseUIView
{
    protected T view;

    private bool isInitialized;
    
    public StateWithUI(FSM fsm) : base(fsm) 
    {
    }

    public sealed override void Enter()
    {
        Initialize();
        base.Enter();
        view.Show();
        
        DoEnter();
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