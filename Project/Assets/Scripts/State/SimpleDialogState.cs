namespace Game.UI
{
    public class SimpleDialogState : StateWithUI<SimpleDialogView>
    {
        public SimpleDialogState(States states, SimpleDialogView view):base(states)
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