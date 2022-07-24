using System.Collections;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using Zenject;

public class GameStarter : MonoBehaviour
{
    [Inject]
    private States states;

    [Inject] private RootState rootState;
    [Inject] private MechState _mechState;
    
    
    // Start is called before the first frame update
    void Start()
    {
        RegisterStateWithUI(_mechState);
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
