using XFlow.Modules.States;
using XFlow.Modules.States.FSM;

namespace Game.State
{
    public class InventoryOpenedState : StateWithUI
    {
        public InventoryOpenedState(FSM fsm) : base(fsm)
        {
        }
    }
}