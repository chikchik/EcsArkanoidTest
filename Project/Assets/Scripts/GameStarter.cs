using Game.State;
using Game.UI;
using Game.View;
using UnityEngine;
using XFlow.Modules.States;
using Zenject;

namespace Game
{
    public class GameStarter : MonoBehaviour
    {
        [Inject]
        private States states;

        [Inject] private RootState rootState;
        [Inject] private MechInfoState mechInfoState;
        [Inject] private InventoryOpenedState inventoryOpenedState;
    
    
        // Start is called before the first frame update
        void Start()
        {
            RegisterStateWithUI(mechInfoState);
            states.RegisterState(inventoryOpenedState);
            
            states.RegisterState(rootState);
        
            states.StartFrom<RootState>();
        }

        private void RegisterStateWithUI<T>(StateWithUI<T> state) where T:BaseUIView
        {
            state.GetView().gameObject.SetActive(false);
            states.RegisterState(state);
        }
    }
}
