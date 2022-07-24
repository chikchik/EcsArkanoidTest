namespace Game.UI
{
    public class SimpleDialogState : StateWithUI<SimpleDialogView>
    {
        public SimpleDialogState(FSM.FSM fsm, SimpleDialogView view):base(fsm)
        {
            this.view = view;
        }

        protected override void DoInitialize()
        {
            view.ButtonClose.onClick.AddListener(() =>
            {
                //or fsm.Pop();
                //or fsm.PopThis(this);
                Close();
            });
        }
    }
}