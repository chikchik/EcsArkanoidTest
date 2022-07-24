using Game.State;
using Game.UI;
using Game.View;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameStarter : MonoBehaviour
    {
        [Inject]
        private States states;

        [Inject] private RootState rootState;
        [Inject] private MechInfoState _mechInfoState;
    
    
        // Start is called before the first frame update
        void Start()
        {
            RegisterStateWithUI(_mechInfoState);
            states.RegisterState(rootState);
        
            states.StartFrom<RootState>();
        }

        private void RegisterStateWithUI<T>(StateWithUI<T> state) where T:BaseUIView
        {
            state.GetView().gameObject.SetActive(false);
            states.RegisterState(state);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
