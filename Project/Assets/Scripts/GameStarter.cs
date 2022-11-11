using Game.State;
using UnityEngine;
using XFlow.Modules.States;
using Zenject;

namespace Game
{
    public class GameStarter : MonoBehaviour
    {
        [Inject] private States _states;
        [Inject] private RootState _rootState;
        [Inject] private MechInfoState _mechInfoState;
        [Inject] private InventoryOpenedState _inventoryOpenedState;
        [Inject] private LoginUIState _loginUiState;

        // Start is called before the first frame update
        private void Start()
        {
            RegisterStateWithUI(_loginUiState);
            RegisterStateWithUI(_mechInfoState);
            _states.RegisterState(_inventoryOpenedState);

            _states.RegisterState(_rootState);

            _states.StartFrom<RootState>();
        }

        private void RegisterStateWithUI<T>(StateWithUI<T> state) where T : BaseUIView
        {
            state.GetView().gameObject.SetActive(false);
            _states.RegisterState(state);
        }
    }
}